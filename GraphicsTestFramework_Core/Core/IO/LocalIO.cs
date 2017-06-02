using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace GraphicsTestFramework
{
	public class LocalIO : MonoBehaviour
	{
		public static LocalIO _Instance = null;

		public static LocalIO Instance {
			get {
				if (_Instance == null)
					_Instance = (LocalIO)FindObjectOfType (typeof(LocalIO));
				return _Instance;
			}
		}

		//SystemData
		private SystemData sysData;
		//Data path for local files
		private string dataPath;
		//prefix for local baseline files
		private string baselinePrefix = "Baseline";
		//prefix for local results files
		private string resultsCurrentPrefix = "ResultsCurrent";
		private bool isWaiting;
		//Total disk space used by local files
		public long spaceUsed;


		public void Init (SystemData systemData)
		{
			#if UNITY_EDITOR
			dataPath = (Application.dataPath).Substring (0, Application.dataPath.Length - 6) + "EditorResults";
			#else
			dataPath = Application.persistentDataPath;
			#endif

			sysData = systemData;

			if (!Directory.Exists (dataPath)) //directory check
				Directory.CreateDirectory (dataPath);
			else {
				spaceUsed = CheckDiskSpace (dataPath, true);
			}
		}

		/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		/// Writing data - TODO wip
		/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------



		/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		/// Writing data - TODO wip
		/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Creates the local from JSON.
		/// </summary>
		/// <returns>The local from cloud.</returns>
		/// <param name="objTypeNames">Object type names.</param>
		/// <param name="jsonData">Json data.</param>
		public IEnumerator CreateLocalFromCloud(List<string> objTypeNames, List<string> jsonData){
			string suite = "";
			string lastSuite = "";
			string testType = "";
			string lastTestType = "";
			string api = "";
			string lastApi = "";
			string pipe = "";
			string lastPipe = "";

			List<string> jsonRows = new List<string>();

			for(int i = 0; i < objTypeNames.Count; i++)
			{
				string[] typeSplit = objTypeNames [i].Split (new char[]{ '_' }, System.StringSplitOptions.None);
				string[] splitJson = jsonData [i].Split (new char[]{',', ':'}, System.StringSplitOptions.None);
				suite = typeSplit [0];
				testType = typeSplit [1];
				api = splitJson [11].Substring (1, splitJson [11].Length-2);
				pipe = splitJson [13].Substring (1, splitJson [13].Length-2);

				if ((lastSuite != suite || lastTestType != testType || lastApi != api || pipe != lastPipe) && jsonRows.Count > 0) {
					isWaiting = true;
					StartCoroutine(WriteDataFiles(lastSuite, lastTestType, JSONHelper.FromJSON (jsonRows.ToArray ()), jsonRows.ToArray (), fileType.Baseline));//REORG
					while (isWaiting)
						yield return null;
					jsonRows.Clear ();
				}

				jsonRows.Add (jsonData[i]);

				lastSuite = suite;
				lastTestType = testType;
				lastApi = api;
				lastPipe = pipe;
			}if(jsonRows.Count > 0){
				isWaiting = true;
				StartCoroutine(WriteDataFiles(lastSuite, lastTestType, JSONHelper.FromJSON (jsonRows.ToArray ()), jsonRows.ToArray (), fileType.Baseline));//REORG
				while (isWaiting)
					yield return null;
				jsonRows.Clear ();
			}
			List<SuiteBaselineData> LocalBaselines = ReadLocalBaselines();

			while (CloudConnectorCore.isWaiting || CloudImagesConnector.responseCount != 0)
				yield return new WaitForEndOfFrame ();

			ResultsIO.Instance.CloudBaselineDataRecieved (LocalBaselines);
		}

		/// <summary>
		/// Writes the data files.
		/// </summary>
		/// <returns>The data files.</returns>
		/// <param name="suite">Suite.</param>
		/// <param name="testType">Test type.</param>
		/// <param name="resultIOdata">Result I odata.</param>
		/// <param name="data">Data.</param>
		/// <param name="filetype">Filetype.</param>
		public IEnumerator WriteDataFiles (string suite, string testType, ResultsIOData resultIOdata, string[] data, fileType filetype)
		{
			if (Master.Instance.debugMode == Master.DebugMode.Messages) {
				Debug.Log ("Beginning to write suite " + suite + " testType " + testType + " which contains " + data.Length + " files to write");
			}
			string filePath = CreateDataDirectory (suite, resultIOdata.resultsRow [0].commonResultsIOData.API, resultIOdata.resultsRow [0].commonResultsIOData.RenderPipe, testType);  //dataPath + "/" + suite + "/" + resultIOdata.resultsRow [1].commonResultsIOData.API + "/" + resultIOdata.resultsRow [1].commonResultsIOData.RenderPipe + "/" + testType;
			string prefix = "InvalidData"; // prefix for whether a results file or baseline file

			int suiteBaselineDataIndex = -1;
			if (filetype == fileType.Baseline) { //if it's a baseline file we need to update latest baseline timesstamp
				prefix = baselinePrefix;

				ResultsIO.Instance.UpdateBaselineDictionary (suite, resultIOdata.resultsRow [0].commonResultsIOData.RenderPipe, out suiteBaselineDataIndex);
			} else {
				prefix = resultsCurrentPrefix;
			}
			//go through all the results data
			int fileCount = 0;
			for (int i = 0; i < data.Length; i++) {// TODO - offset by one, single result always comes 
				ResultsDataCommon commonData = resultIOdata.resultsRow [i].commonResultsIOData;
				//write data to files
				if (!Directory.Exists (filePath)) // check to see ig folder exists if not create it
					Directory.CreateDirectory (filePath);
				string fileName = prefix + "_" + commonData.SceneName + "_" + commonData.TestName + ".txt";
				File.WriteAllText (filePath + "/" + fileName, data [i]);
				while (!File.Exists (filePath + "/" + fileName)) {
					if (Master.Instance.debugMode == Master.DebugMode.Messages)
						Debug.Log ("Writing...");
					yield return new WaitForEndOfFrame ();
				}
				//update baseline dictionary(not a dictionary) if needed
				if(filetype == fileType.Baseline){
					ResultsIO.Instance.BaselineDictionaryEntry (suiteBaselineDataIndex, testType, commonData.SceneName, commonData.TestName, commonData.DateTime);
				}

				fileCount++;
			}
			if (Master.Instance.debugMode == Master.DebugMode.Messages)
				Debug.Log ("Wrote " + fileCount + " files to disk");

			//Write baseline dictionary for suite and update timestamp TODO might need work/tweaking
			if (filetype == fileType.Baseline) {
				StartCoroutine (UpdateSuiteDataFiles());
			}

			yield return new WaitForEndOfFrame ();
		}

		/// <summary>
		/// Updates the suite data files.
		/// </summary>
		/// <returns>The suite data files.</returns>
		IEnumerator UpdateSuiteDataFiles ()
		{
			foreach(SuiteBaselineData SBD in ResultsIO.Instance._suiteBaselineData){
				string filePath = dataPath + "/" + SBD.suiteName + "/" + SBD.api;
				if (!Directory.Exists (filePath)) // check to see ig folder exists if not create it
					Directory.CreateDirectory (filePath);
				string fileName = "SuiteData_" + SBD.suiteName + "_" + SBD.api + "_" + SBD.pipeline + ".txt";

				List<string> newFileContent= new List<string>();
				newFileContent.Add (System.DateTime.UtcNow.ToString ());

				foreach(SuiteData SD in SBD._suiteData){
					//newFileContent.Add(SBD.pipeline);
					newFileContent.Add (JsonUtility.ToJson (SD));
				}
				File.WriteAllLines (filePath + "/" + fileName, newFileContent.ToArray ());
				while (CloudConnectorCore.isWaiting)
					yield return null;
				CloudIO.Instance.SetBaselineTimestamp (SBD);// - todo
				yield return null;
			}isWaiting = false;
		}

		/// <summary>
		/// Write large files to EXTERNAL_DATA folder to replicate cloud sorting.
		/// </summary>
		/// <param name="value">Contents to write.</param>
		/// <param name="key">Key for file(also file name).</param>
		public void LargeFileWrite(string value, string key){
			string filePath = dataPath + "/EXTERNAL_DATA";
			string fileName = key + ".txt";
			if (!Directory.Exists (filePath))
				Directory.CreateDirectory (filePath);
			File.WriteAllText (filePath + "/" + fileName, value);
		}

		/// <summary>
		/// Creates the data directory string and local directory file for test data.
		/// </summary>
		/// <returns>The data directory as a string.</returns>
		/// <param name="suite">Suite.</param>
		/// <param name="api">API.</param>
		/// <param name="renderPipe">Render pipe.</param>
		/// <param name="testType">Test type.</param>
		public string CreateDataDirectory(string suite, string api, string renderPipe, string testType){
			string filePath = dataPath + "/" + suite + "/" + api + "/" + renderPipe + "/" + testType; // format the folder hierachy

			if (Master.Instance.debugMode == Master.DebugMode.Messages)
				Debug.Log ("Checking directory:" + filePath);

			if (!Directory.Exists (filePath)) // check to see if folder exists if not create it
				Directory.CreateDirectory (filePath);
			return filePath; // return the 
		}

		/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		/// Reading data - TODO wip
		/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Populates the local baseline lookup.
		/// </summary>
		public List<SuiteBaselineData> ReadLocalBaselines(){
			//check suites>api>pipelines
			List<SuiteBaselineData> baselineData = new List<SuiteBaselineData>();
			string[] suites = Directory.GetDirectories (dataPath);
			foreach(string s in suites){
				string[] apis = Directory.GetDirectories (s);
				foreach(string api in apis){
					string[] pipes = Directory.GetDirectories (api);
					foreach(string pipe in pipes){
						string fileName = "SuiteData_" + Path.GetFileName (s) + "_" + Path.GetFileName (api) + "_" + Path.GetFileName (pipe) + ".txt";
						if (File.Exists (api + "/" + fileName)) {
							SuiteBaselineData SBD = new SuiteBaselineData ();
							SBD.suiteName = Path.GetFileName (s);
							SBD.platform = sysData.Platform;
							SBD.api = Path.GetFileName (api);
							SBD.pipeline = Path.GetFileName (pipe);
							string[] fileLines = File.ReadAllLines (api + "/" + fileName);
							SBD.suiteTimestamp = fileLines[0];
							List<SuiteData> sData = new List<SuiteData>();
							for(int i = 1; i < fileLines.Length; i++){
								sData.Add (JsonUtility.FromJson<SuiteData> (fileLines[i]));
							}
							SBD._suiteData = sData;
							baselineData.Add (SBD);
						}
					}
				}
			}
			return baselineData;
		}

		/// <summary>
		/// Fetchs the local data file.
		/// </summary>
		/// <returns>The data file.</returns>
		/// <param name="suite">Suite.</param>
		/// <param name="testType">Test type.</param>
		/// <param name="resultsDataCommon">Results data common.</param>
		/// <param name="baseline">If set to <c>true</c> baseline.</param>
		public string FetchDataFile (string suite, string testType, ResultsDataCommon resultsDataCommon, bool baseline)
		{
			if (Master.Instance.debugMode == Master.DebugMode.Messages) {
				Debug.Log ("Beginning fetch process");
			}

			string filePath = dataPath + "/" + suite + "/" + resultsDataCommon.API + "/" + resultsDataCommon.RenderPipe + "/" + testType;
			string fileName = "_" + resultsDataCommon.SceneName + "_" + resultsDataCommon.TestName + ".txt";

			if (baseline)
				fileName = baselinePrefix + fileName;
			else
				fileName = resultsCurrentPrefix + fileName;

			if (!Directory.Exists (filePath)) {
				if (Master.Instance.debugMode == Master.DebugMode.Messages) {
					Debug.LogWarning ("Directory for baseline does not exist, please pull latest baselines or create them");
				}
				return null;
			} else {
				if (Master.Instance.debugMode == Master.DebugMode.Messages) {
					Debug.Log ("Directory for baseline exists, attempting to fetch requested baseline");
				}
				if (!File.Exists (filePath + "/" + fileName)) {
					if (Master.Instance.debugMode == Master.DebugMode.Messages) {
						Debug.LogWarning ("Baseline file does not exist for the requested test, please make sure you pull the latest or create them");
					}
					return null;
				} else {
					return File.ReadAllText (filePath + "/" + fileName);
				}
			}
		}

		/// <summary>
		/// Read large file in EXTERNAL_DATA folder.
		/// </summary>
		/// <param name="key">Key for file(also file name).</param>
		public string LargeFileRead(string key){
			string filePath = dataPath + "/EXTERNAL_DATA";
			string fileName = key + ".txt";
			if (!File.Exists (filePath + "/" + fileName))
				return null;
			else{
				return File.ReadAllText (filePath + "/" + fileName);
			}
		}

		/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		/// Utilities - TODO wip
		/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Checks the used disk space of specified path.
		/// </summary>
		/// <returns>The disk space.</returns>
		/// <param name="dir">Directory to check.</param>
		/// <param name="recursive">If set to <c>true</c> include full subfolder hierachy.</param>
		public long CheckDiskSpace (string dir, bool recursive)
		{
			long bytes = new long ();
			DirectoryInfo dirInfo = new DirectoryInfo (dir);
			FileInfo[] fileInfo = dirInfo.GetFiles ();
			DirectoryInfo[] dirInfos = dirInfo.GetDirectories ();
			foreach (FileInfo fi in fileInfo) {
				bytes += fi.Length;
			}
			if(recursive) {
				foreach (DirectoryInfo di in dirInfos) {
					bytes += CheckDiskSpace (di.FullName, true);
				}
			}
			return bytes;
		}

	}
}
