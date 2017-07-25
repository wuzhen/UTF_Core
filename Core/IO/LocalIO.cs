using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Beginning to write suite " + suite + " testType " + testType + " which contains " + data.Length + " files to write"); // Write to console
            string platformAPI = resultIOdata.resultsRow[0].commonResultsIOData.Platform + "_" + resultIOdata.resultsRow[0].commonResultsIOData.API;
            string filePath = CreateDataDirectory (suite, platformAPI, resultIOdata.resultsRow [0].commonResultsIOData.RenderPipe, testType);  //dataPath + "/" + suite + "/" + resultIOdata.resultsRow [1].commonResultsIOData.API + "/" + resultIOdata.resultsRow [1].commonResultsIOData.RenderPipe + "/" + testType;
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
				string fileName = prefix + "_" + commonData.GroupName + "_" + commonData.TestName + ".txt";
				File.WriteAllText (filePath + "/" + fileName, data [i]);
				while (!File.Exists (filePath + "/" + fileName)) {
                    Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Writing..."); // Write to console
					yield return new WaitForEndOfFrame ();
				}
				//update baseline dictionary(not a dictionary) if needed
				if(filetype == fileType.Baseline){
					ResultsIO.Instance.BaselineDictionaryEntry (suiteBaselineDataIndex, testType, commonData.GroupName, commonData.TestName, commonData.DateTime);
				}

				fileCount++;
			}
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Wrote " + fileCount + " files to disk"); // Write to console
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
				string filePath = dataPath + "/" + SBD.suiteName + "/" + SBD.platform + "_" + SBD.api;
				if (!Directory.Exists (filePath)) // check to see ig folder exists if not create it
					Directory.CreateDirectory (filePath);
				string fileName = "SuiteData_" + SBD.suiteName + "_" + SBD.platform + "_" + SBD.api + "_" + SBD.pipeline + ".txt";

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
		/// <param name="platformAPI">API.</param>
		/// <param name="renderPipe">Render pipe.</param>
		/// <param name="testType">Test type.</param>
		public string CreateDataDirectory(string suite, string platformAPI, string renderPipe, string testType){
			string filePath = dataPath + "/" + suite + "/" + platformAPI + "/" + renderPipe + "/" + testType; // format the folder hierachy
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Checking directory:" + filePath); // Write to console
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
				string[] platformApis = Directory.GetDirectories (s);
				foreach(string platformApi in platformApis)
                {
					string[] pipes = Directory.GetDirectories (platformApi);
					foreach(string pipe in pipes){
						string fileName = "SuiteData_" + Path.GetFileName (s) + "_" + Path.GetFileName (platformApi) + "_" + Path.GetFileName (pipe) + ".txt";
						if (File.Exists (platformApi + "/" + fileName)) {
							SuiteBaselineData SBD = new SuiteBaselineData ();
							SBD.suiteName = Path.GetFileName (s);
                            string x = Path.GetFileName(platformApi); // Get platformAPI filename
                            int apiIndex = x.LastIndexOf('_'); // Find start index of API
                            SBD.platform = x.Substring(0, apiIndex); // Get substring of platform
							SBD.api = x.Substring(apiIndex + 1); // Get substring of API
							SBD.pipeline = Path.GetFileName (pipe);
							string[] fileLines = File.ReadAllLines (platformApi + "/" + fileName);
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
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Beginning fetch process"); // Write to console
			string filePath = dataPath + "/" + suite + "/" + resultsDataCommon.Platform + "_" + resultsDataCommon.API + "/" + resultsDataCommon.RenderPipe + "/" + testType;
			string fileName = "_" + resultsDataCommon.GroupName + "_" + resultsDataCommon.TestName + ".txt";

			if (baseline)
				fileName = baselinePrefix + fileName;
			else
				fileName = resultsCurrentPrefix + fileName;

			if (!Directory.Exists (filePath)) {
                Console.Instance.Write(DebugLevel.Critical, MessageLevel.Log, "Directory for baseline does not exist, please pull latest baselines or create them"); // Write to console
				return null;
			} else {
                Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Directory for baseline exists, attempting to fetch requested baseline"); // Write to console
				if (!File.Exists (filePath + "/" + fileName)) {
                    Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Baseline file does not exist for the requested test, please make sure you pull the latest or create them"); // Write to console
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

		public void ClearLocalData(){
			Directory.Delete (dataPath, true);
			Directory.CreateDirectory (dataPath);
            Button[] buttons = new Button[2]; // Create button array
            bool openDialogue = Dialogue.Instance.TryDialogue(true, 2, out buttons); // Try for dialogue window and out buttons
            if (openDialogue) // If dialogue opens
            {
                buttons[0].onClick.AddListener(delegate { ResultsIO.Instance.Restart(); }); // Add listeners
                buttons[0].onClick.AddListener(delegate { Dialogue.Instance.SetState(false, 2); });
                buttons[1].onClick.AddListener(delegate { Master.Instance.ExitApplication(); }); // Add listeners
                buttons[1].onClick.AddListener(delegate { Dialogue.Instance.SetState(false, 2); });
            }
            else
                ResultsIO.Instance.Restart(); // Save baseline
        }

        
	}
}
