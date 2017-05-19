using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

namespace GraphicsTestFramework
{
	public class ResultsIO : MonoBehaviour
	{
		private static ResultsIO _Instance = null;
		public long spaceUsed;
		private string dataPath;
		private string baselinePrefix = "Baseline";
		private string resultsCurrentPrefix = "ResultsCurrent";
		private List<string> suiteBaselinesPullList = new List<string>();
		public bool isWaiting = false;

		//cloud specific variables
		private bool cloudMode;

		//List of suiteBaselineData for suites
		public List<SuiteBaselineData> _suiteBaselineData = new List<SuiteBaselineData>(); //TODO make private once working

		//Setup cloud listners
		void OnEnable()
		{
			// Suscribe for catching cloud responses.
			CloudConnectorCore.processedResponseCallback.AddListener(ParseCloudData);
		}

		void OnDisable()
		{
			// Remove listeners.
			CloudConnectorCore.processedResponseCallback.RemoveListener(ParseCloudData);
		}

		private IEnumerator Start ()
		{
            #if UNITY_EDITOR
            dataPath = (Application.dataPath).Substring (0, Application.dataPath.Length - 6) + "EditorResults";
			cloudMode = true;
			#else
			dataPath = Application.persistentDataPath;
			cloudMode = false;
			#endif

			if (!Directory.Exists (dataPath)) //directory check
				Directory.CreateDirectory (dataPath);
			else {
				spaceUsed = CheckDiskSpace (dataPath);
			}

			isWaiting = true;
			ReadLocalBaselines ();// - TODO this needs to get called again at some point
			while(isWaiting == true){
				yield return new WaitForEndOfFrame ();
			}

			float timeout = 0;
			while(SuiteManager.Instance.GetSuiteNames().Length == 0){
				timeout += Time.deltaTime;
				if(timeout > 5f){
					
					break;
				}
				yield return new WaitForEndOfFrame();
			}

			string[] suiteNames = SuiteManager.Instance.GetSuiteNames ();
			if(suiteNames.Length == 0){
				Debug.LogWarning("No suites loaded in SuiteManager, unable to continue");
			}else{
				foreach(string suiteName in suiteNames){
					GetBaselineTimestamp (suiteName);
					while(CloudConnectorCore.isWaiting == true){
						yield return new WaitForEndOfFrame ();
					}
				}

				yield return new WaitForSeconds(1f);

				if(suiteBaselinesPullList.Count > 0)
					FetchCloudBaselines (suiteBaselinesPullList.ToArray ());
				else{
					Debug.LogWarning("No cloud based baselines to pull");
					BroadcastBaselineParsed ();
				}
			}

		}

		public static ResultsIO Instance {
			get {
				if (_Instance == null)
					_Instance = (ResultsIO)FindObjectOfType (typeof(ResultsIO));
				return _Instance;
			}
		}

		/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		/// Initial checks
		/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		//Check BaseData for Suite timestamps, set off fetch baselines

		/// <summary>
		/// Query if specific baseline exists
		/// </summary>
		/// <returns><c>true</c>, if baseline exists, <c>false</c> if not.</returns>
		/// <param name="suiteName">Suite name.</param>
		/// <param name="pipeline">Pipeline.</param>
		/// <param name="testType">Test type.</param>
		/// <param name="sceneName">Scene name.</param>
		/// <param name="testName">Test name.</param>
		public bool BaselineExists(string suiteName, string pipeline, string testType, string sceneName, string testName){
			//check local file for current platform/api baseline
			foreach(SuiteBaselineData SBD in _suiteBaselineData){
				if(SBD.suiteName == suiteName){
					//suite exists
					if(SBD.api == Master.Instance.GetSystemData ().API){
						//API matches
						if(SBD.pipeline == pipeline){
							//pipeline exists
							foreach(SuiteData SD in SBD._suiteData){
								if (SD.testType == testType && SD.sceneName == sceneName && SD.testName == testName) {
									return true;
								}
							}
						}
					}
				}
			}
			return false;
		}

		void CompareBaselineTimestamps(string suiteName, string dateTime){
			System.DateTime cloudTimestamp = System.DateTime.Parse (dateTime);

			if(_suiteBaselineData.Count == 0){
				Debug.Log("putting it in the pull list");
				suiteBaselinesPullList.Add(suiteName);
			}else
			{
				foreach(SuiteBaselineData SBD in _suiteBaselineData){
					if (SBD.suiteName == suiteName) {
						System.DateTime localTimestamp = System.DateTime.Parse (SBD.suiteTimestamp);

						int timeDiff = cloudTimestamp.CompareTo (localTimestamp);
						if (timeDiff < 0f) {
							Debug.Log ("Cloud Timestamp is old");
						} else if (timeDiff > 0f) {
							if(Master.Instance.debugMode == Master.DebugMode.Messages)
								Debug.Log ("Cloud Timestamp is newer, adding " + suiteName + " to pull list");
							suiteBaselinesPullList.Add (suiteName);
						} else if (timeDiff == 0f) {
							Debug.Log ("Cloud Timestamp is the same"); // never gonna happen
						}
					}
				}
			}
		}


		/// <summary>
		/// /*Pull relevant baseline data from cloud.*/
		/// </summary>
		void FetchCloudBaselines(string[] suiteNames){
            ProgressScreen.Instance.SetState(true, ProgressType.CloudLoad, "Retrieving cloud data");
            SystemData sysData = Master.Instance.GetSystemData ();
			Debug.Log ("getting baselines for " + sysData.Platform + " on API " + sysData.API);
			foreach (string s in suiteNames)
				Debug.Log ("pulling suite=" + s);
			CloudConnectorCore.GetBaselineData (suiteNames, sysData.Platform, sysData.API, cloudMode);
		}

		/// <summary>
		/// Populates the local baseline lookup.
		/// </summary>
		void ReadLocalBaselines(){
            ProgressScreen.Instance.SetState(true, ProgressType.LocalLoad, "Loading local data");
            //check suites>api>pipelines
            string[] suites = Directory.GetDirectories (dataPath);
			foreach(string s in suites){
				string[] apis = Directory.GetDirectories (s);
				foreach(string api in apis){
					string[] pipes = Directory.GetDirectories (api);
					foreach(string pipe in pipes){
						string fileName = "SuiteData_" + Path.GetFileName (s) + "_" + Path.GetFileName (api) + "_" + Path.GetFileName (pipe) + ".txt";
						if (File.Exists (api + "/" + fileName)) {
							SuiteBaselineData SBD = new SuiteBaselineData ();
							SystemData SD = Master.Instance.GetSystemData ();
							SBD.suiteName = Path.GetFileName (s);
							SBD.platform = SD.Platform;
							SBD.api = Path.GetFileName (api);
							SBD.pipeline = Path.GetFileName (pipe);
							string[] fileLines = File.ReadAllLines (api + "/" + fileName);
							SBD.suiteTimestamp = fileLines[0];
							List<SuiteData> sData = new List<SuiteData>();
							for(int i = 1; i < fileLines.Length; i++){
								sData.Add (JsonUtility.FromJson<SuiteData> (fileLines[i]));
							}
							SBD._suiteData = sData;
							_suiteBaselineData.Add (SBD);
						}
					}
 				}
			}
			isWaiting = false;
			//BroadcastBaselineParsed ();
		}

		/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		/// Processing fresh test data
		/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Processes the results, saving local txt files and pushing to cloud.
		/// </summary>
		/// <param name="suiteName">Suite name.</param>
		/// <param name="testType">Test type.</param>
		/// <param name="inputData">Input data.</param>
		/// <param name="baseline">Baseline data?</param>
		public void ProcessResults (string suiteName, string testType, ResultsIOData inputData, int baseline)
		{
			string[] data = ToJSON (inputData);

			fileType ft;
			if (baseline == 1) {
				ft = fileType.Baseline;
				//Cloud upload for baseline
				string sheetName = suiteName + "_" + testType + "_Baseline";

				StartCoroutine (UploadData (data, sheetName, baseline, inputData.resultsRow [0].resultsColumn.ToArray ()));
			} else {
				ft = fileType.Result;
				//cloud upload for results
				string sheetName = suiteName + "_" + testType + "_Results";
				StartCoroutine (UploadData (data, sheetName, baseline, inputData.resultsRow [0].resultsColumn.ToArray ()));;
			}

			if (data != null) {
				inputData.resultsRow.RemoveAt (0);

				for(int i = 0; i < data.Length - 1; i++){
					data [i] = data [i + 1];
				}
				Array.Resize<string>(ref data, data.Length-1);
				StartCoroutine (WriteDataFiles (suiteName, testType, inputData, data, ft));
			}
            else
            {
                Debug.LogWarning("Results are empty for Suite: "+suiteName+" Type: "+ testType + ". Nothing to write");
                BroadcastEndResultsSave();
            }
		}

		/// <summary>
		/// Retrieves a result file.
		/// </summary>
		/// <returns>The result.</returns>
		/// <param name="suiteName">Suite name.</param>
		/// <param name="testType">Test type.</param>
		/// <param name="inputData">Input data.</param>
		public ResultsIOData RetrieveResult (string suiteName, string testType, ResultsDataCommon inputData)
		{
			string rawJSONdata = FetchDataFile (suiteName, testType, inputData, false);//fetch string from file
			ResultsIOData data = FromJSON (rawJSONdata);//take JSON convert to ResultsIOData
			return data;
		}

		/// <summary>
		/// Retrieves a baseline file.
		/// </summary>
		/// <returns>The baseline.</returns>
		/// <param name="suiteName">Suite name.</param>
		/// <param name="testType">Test type.</param>
		/// <param name="inputData">Input data.</param>
		public ResultsIOData RetrieveBaseline(string suiteName, string testType, ResultsDataCommon inputData)
		{
			string rawJSONdata = FetchDataFile (suiteName, testType, inputData, true);//fetch string from file
			ResultsIOData data = FromJSON (rawJSONdata);//take JSON convert to ResultsIOData
			return data;
		}

		/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		/// Cloud intergration - TODO not started
		/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		//Get baseline timestamp
		void GetBaselineTimestamp(string suiteName){
            ProgressScreen.Instance.SetState(true, ProgressType.CloudLoad, "Checking for inconsistencies with cloud data");
            CloudConnectorCore.GetObjectsByField ("SuiteBaselineTimestamps", "suiteName", suiteName, cloudMode);
		}

		void ProcessBaselineTimestamp(List<string> objNames, List<string> jsonData){ // TODO- externalize JSON splitting for multiple json arrays from cloudconnector
			if(jsonData[0].Length > 2){
				jsonData[0] = jsonData [0].Remove (0, 1);
				jsonData [0] = jsonData [0].Remove (jsonData [0].Length - 1, 1);
				string[] separators = new string[]{ "},{" };
				string[] splitJson = jsonData [0].Split (separators, System.StringSplitOptions.None);


				for(int i = 0; i< splitJson.Length; i++){
					string jsonString = splitJson [i];
					if (splitJson.Length > 1) {
						if (i == 0)
							jsonString += "}";
						else if (i == splitJson.Length - 1)
							jsonString = "{" + jsonString;
						else
							jsonString = "{" + jsonString + "}";
					}
					Dictionary<string, string> jsonDic = JSON_Dictionary (jsonString);
					if(jsonDic["api"] == Master.Instance.GetSystemData ().API){
						CompareBaselineTimestamps (jsonDic["suiteName"], jsonDic["suiteTimestamp"]);
					}
				}

			}isWaiting = false;
		}

		//Set baseline timestamp
		void SetBaselineTimestamp (SuiteBaselineData SBD)
		{
			//update suite timestamp and baseline timestamp
			//string[] headers = new string[]{"suiteName", "platform", "api", "suiteTimestamp"};
 			//CloudConnectorCore.CreateTable (headers, "SuiteBaselineTimestamps", cloudMode);// TODO - may never need again but should resolve
			Dictionary<string, string> entry = new Dictionary<string, string> ();

			entry ["suiteName"] = SBD.suiteName;
			entry ["platform"] = SBD.platform;
			entry ["api"] = SBD.api;
			entry ["suiteTimestamp"] = SBD.suiteTimestamp;

			CloudConnectorCore.UpdateUniqueRow ("SuiteBaselineTimestamps", new int[]{1, 2, 3},  ToJSON (entry), cloudMode);
		}

		/// <summary>
		/// Uploads the data.
		/// Send results with baseline switch TODO - need to make it so baseline files override if exist otherwise add(maybe new cloudconnector function)
		/// </summary>
		/// <returns>The data.</returns>
		/// <param name="jsonData">Json data array</param>
		/// <param name="sheetName">Sheet name.</param>
		/// <param name="baseline">Baseline.</param>
		IEnumerator UploadData(string[] jsonData, string sheetName, int baseline, string[] fields){
			if(Master.Instance.debugMode == Master.DebugMode.Messages)
				Debug.Log ("Uploading JSON data to sheet=" + sheetName);
			float uploadStartTime = Time.realtimeSinceStartup;

			string[] trimmedJson = new string[jsonData.Length - 1];
			for(int i = 1; i < jsonData.Length; i++){
				trimmedJson [i - 1] = jsonData [i];
			}
			CreateWorksheet (fields, sheetName);
			//wait for cloudcore to be idle
			while (CloudConnectorCore.isWaiting)
				yield return null;

			if (baseline == 1) {
				//replace each individually
				for(int i = 0; i < trimmedJson.Length; i++){
					//update rows based on matching columns 4, 5, 6, 7, 8 > Platform, API, RenderPipe, SceneName, TestName
					CloudConnectorCore.UpdateUniqueRow (sheetName, new int[]{4, 5, 6, 7, 8}, trimmedJson[i], cloudMode);
					//wait for cloudcore to be idle
					while (CloudConnectorCore.isWaiting)
						yield return null;
				}
			} else {
				//upload the array of json objects
				CloudConnectorCore.CreateObjects (trimmedJson, sheetName, cloudMode);
			}
            //wait for cloudcore to be idle
            while (CloudConnectorCore.isWaiting)
				yield return null;
			//Debug the time it took to upload
			if(Master.Instance.debugMode == Master.DebugMode.Messages){
				Debug.Log ("Upload of " + (jsonData.Length - 1) + " items in " + (Time.realtimeSinceStartup - uploadStartTime) + "ms");
			}
            ProgressScreen.Instance.SetState(false, ProgressType.LocalSave, "");
        }

		//Get baselines

		/// <summary>
		/// Checks the worksheet exists.
		/// </summary>
		/// <param name="sheetName">Sheet name.</param>
		public void CheckWorksheet(string sheetName){
			CloudConnectorCore.TableExists (sheetName, cloudMode);
		}

		/// <summary>
		/// Creates the worksheet.
		/// </summary>
		/// <param name="headers">Worksheet headers.</param>
		/// <param name="sheetName">Sheet name.</param>
		void CreateWorksheet(string[] headers, string sheetName){
			CloudConnectorCore.CreateTable (headers, sheetName, cloudMode);
		}


		/// <summary>
		/// Parses the cloud data.
		/// </summary>
		/// <param name="query">Query type</param>
		/// <param name="objTypeNames">Object type names.</param>
		/// <param name="jsonData">Json data.</param>
		void ParseCloudData(CloudConnectorCore.QueryType query, List<string> objTypeNames, List<string> jsonData){

			if (query == CloudConnectorCore.QueryType.tableExists) {
				Debug.Log (jsonData [0]);
			}

			if (query == CloudConnectorCore.QueryType.getBaselineData) {
				StartCoroutine (CreateLocalFromCloud (objTypeNames, jsonData));
			}

			if(query == CloudConnectorCore.QueryType.getObjects){
				if (objTypeNames [0] == "SuiteBaselineTimestamps"){
					if(jsonData[0].Length < 4)
						Debug.LogWarning("Baseline timestamp does not exist in cloud");
					else
						ProcessBaselineTimestamp (objTypeNames, jsonData);
				}
			}

		}

		IEnumerator CreateLocalFromCloud(List<string> objTypeNames, List<string> jsonData){
			string suite = "";
			string lastSuite = "";
			string testType = "";
			string lastTestType = "";
			string api = "";
			string lastApi = "";
			string pipe = "";
			string lastPipe = "";

			List<string> jsonRows = new List<string>();

			foreach (string s in objTypeNames)
				Debug.Log (s);

			for(int i = 0; i < objTypeNames.Count; i++)
			{
				string[] typeSplit = objTypeNames [i].Split (new char[]{ '_' }, System.StringSplitOptions.None);
				string[] splitJson = jsonData [i].Split (new char[]{',', ':'}, System.StringSplitOptions.None);
				suite = typeSplit [0];
				testType = typeSplit [1];
				api = splitJson [11].Substring (1, splitJson [11].Length-2);
				pipe = splitJson [13].Substring (1, splitJson [13].Length-2);

				Debug.LogWarning (suite + "|" + testType + "|" + api + "|" + pipe);
				if ((lastSuite != suite || lastTestType != testType || lastApi != api || pipe != lastPipe) && jsonRows.Count > 0) {
					isWaiting = true;
					StartCoroutine(WriteDataFiles(lastSuite, lastTestType, FromJSON (jsonRows.ToArray ()), jsonRows.ToArray (), fileType.Baseline));
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
				StartCoroutine(WriteDataFiles(lastSuite, lastTestType, FromJSON (jsonRows.ToArray ()), jsonRows.ToArray (), fileType.Baseline));
				while (isWaiting)
					yield return null;
				jsonRows.Clear ();
			}
            isWaiting = true;
            ReadLocalBaselines();
            while (isWaiting == true)
                yield return null;
            BroadcastBaselineParsed();
        }


		/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		/// Utilities - TODO wip
		/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		//Fetch result/baseline data file as a JSON string
		string FetchDataFile (string suite, string testType, ResultsDataCommon resultsDataCommon, bool baseline)
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

		//WriteBaseData


		/// <summary>
		/// /*Updates the suite data files.*/
		/// </summary>
		/// <returns>The suite data files are written to disk based off the baseline dictionary.</returns>
		IEnumerator UpdateSuiteDataFiles ()
		{
			foreach(SuiteBaselineData SBD in _suiteBaselineData){
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
				SetBaselineTimestamp (SBD);// - todo
				yield return null;
			}isWaiting = false;
		}

		/// <summary>
		/// /*Creates the data directory string and local directory file.*/
		/// </summary>
		/// <returns>The data directory as a string.</returns>
		/// <param name="suite">Suite.</param>
		/// <param name="api">API.</param>
		/// <param name="renderPipe">Render pipe.</param>
		/// <param name="testType">Test type.</param>
		string CreateDataDirectory(string suite, string api, string renderPipe, string testType){
			string filePath = dataPath + "/" + suite + "/" + api + "/" + renderPipe + "/" + testType; // format the folder hierachy

			if (Master.Instance.debugMode == Master.DebugMode.Messages)
				Debug.Log ("Checking directory:" + filePath);

			if (!Directory.Exists (filePath)) // check to see if folder exists if not create it
				Directory.CreateDirectory (filePath);
			return filePath; // return the 
		}

		/// <summary>
		/// Updates the baseline dictionary(base not individual baselines, call this first then BaselineDictionaryEntry).
		/// This has an out of an int for knowing what index the particular suite is located in the list
		/// </summary>
		/// <param name="suite">Suite.</param>
		/// <param name="renderPipe">Render pipe.</param>
		/// <param name="baselineIndex">Baseline index in the list.</param>
		void UpdateBaselineDictionary(string suite, string renderPipe, out int baselineIndex){
			//Updates suiteListData, we need to find the matching suiteListData or create a new one
			int suiteBaselineDataIndex = -1;
			int suiteIndex = 0;
			foreach(SuiteBaselineData SBD in _suiteBaselineData){
				if(SBD.platform == Application.platform.ToString () && SBD.suiteName == suite && SBD.api == Master.Instance.GetSystemData ().API && SBD.pipeline == renderPipe){
					suiteBaselineDataIndex = suiteIndex;
				}suiteIndex++;
			}
			if(suiteBaselineDataIndex == -1){
				SuiteBaselineData newSBD = new SuiteBaselineData();
				newSBD.api = Master.Instance.GetSystemData ().API;
				newSBD.pipeline = renderPipe;
				newSBD.platform = Application.platform.ToString ();
				newSBD.suiteName = suite;
				newSBD.suiteTimestamp = System.DateTime.UtcNow.ToString ();
				_suiteBaselineData.Add (newSBD);
				suiteBaselineDataIndex = _suiteBaselineData.Count-1;
			}
			_suiteBaselineData [suiteBaselineDataIndex].suiteTimestamp = System.DateTime.UtcNow.ToString ();
			//Debug.Log ("Passing SDB index of " + suiteBaselineDataIndex);
			baselineIndex = suiteBaselineDataIndex;
		}

		/// <summary>
		/// Creates a baseline dictionary entry.
		/// </summary>
		/// <param name="suiteIndex">Suite index.</param>
		/// <param name="testType">Test type.</param>
		/// <param name="sceneName">Scene name.</param>
		/// <param name="testName">Test name.</param>
		/// <param name="dateTime">Date time.</param>
		void BaselineDictionaryEntry(int suiteIndex, string testType, string sceneName, string testName, string dateTime){
			
			SuiteData sData = new SuiteData (testType, sceneName, testName, dateTime);
			int check = -1;
			for(int si = 0; si < _suiteBaselineData [suiteIndex]._suiteData.Count; si++){
				SuiteData SD = _suiteBaselineData [suiteIndex]._suiteData[si];
				if (SD.testType == testType && SD.sceneName == sceneName && SD.testName == testName) {
					_suiteBaselineData [suiteIndex]._suiteData[si] = sData;
					check++;
				}
			}
			if (check == -1) {
				_suiteBaselineData [suiteIndex]._suiteData.Add (sData);
			}
			_suiteBaselineData [suiteIndex].suiteTimestamp = System.DateTime.UtcNow.ToString ();
		}

		//WriteBaseline or Results
		IEnumerator WriteDataFiles (string suite, string testType, ResultsIOData resultIOdata, string[] data, fileType filetype)
		{
			if (Master.Instance.debugMode == Master.DebugMode.Messages) {
				Debug.Log ("Beginning to write suite " + suite + " testType " + testType + " which contains " + data.Length + " files to write");
			}
			string filePath = CreateDataDirectory (suite, resultIOdata.resultsRow [0].commonResultsIOData.API, resultIOdata.resultsRow [0].commonResultsIOData.RenderPipe, testType);  //dataPath + "/" + suite + "/" + resultIOdata.resultsRow [1].commonResultsIOData.API + "/" + resultIOdata.resultsRow [1].commonResultsIOData.RenderPipe + "/" + testType;
			string prefix = "InvalidData"; // prefix for whether a results file or baseline file

			int suiteBaselineDataIndex = -1;
			if (filetype == fileType.Baseline) { //if it's a baseline file we need to update latest baseline timesstamp
				prefix = baselinePrefix;

				UpdateBaselineDictionary (suite, resultIOdata.resultsRow [0].commonResultsIOData.RenderPipe, out suiteBaselineDataIndex);
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
					BaselineDictionaryEntry (suiteBaselineDataIndex, testType, commonData.SceneName, commonData.TestName, commonData.DateTime);
				}

				fileCount++;
			}
			if (Master.Instance.debugMode == Master.DebugMode.Messages)
				Debug.Log ("Wrote " + fileCount + " files to disk");

			//Write baseline dictionary for suite and update timestamp TODO might need work/tweaking
			if (filetype == fileType.Baseline) {
				StartCoroutine (UpdateSuiteDataFiles());
			}

			BroadcastEndResultsSave ();

			yield return new WaitForEndOfFrame ();
		}

		Dictionary<string, string> JSON_Dictionary(string inputData){
			string[] separators = new string[]{ "\",\"", "\":\"" };//split by the two JSON separators
			string[] splitData = inputData.Substring (2, inputData.Length - 4).Split (separators, System.StringSplitOptions.None);//remove curly brackets
			Dictionary<string, string> convertedJSON = new Dictionary<string, string> ();

			for (int i = 0; i < splitData.Length/2; i ++) {
				convertedJSON.Add (splitData [i * 2], splitData [(i * 2) + 1]);
			}
			return convertedJSON;
		}

		//Deserialize single JSON > ResultsIOData
		ResultsIOData FromJSON (string inputData)
		{
			if (inputData != null) {
				string[] separators = new string[]{ "\",\"", "\":\"" };//split by the two JSON separators
				string[] splitData = inputData.Substring (2, inputData.Length - 5).Split (separators, System.StringSplitOptions.None);//remove curly brackets
				ResultsIOData data = new ResultsIOData ();//new ResultsIOData
				data.resultsRow.Add (new ResultsIORow ());

				for (int i = 0; i < splitData.Length; i++) {
					int cur = i;
					//Debug.Log (cur + " data=" + splitData [cur]); - TODO remove if works
					/*if (i == 18)//inject the common class reference point
				{
					data.resultsRow [0].resultsColumn.Add ("common");
					data.resultsRow [0].resultsColumn.Add ("GraphicsTestFramework.ResultsDataCommon");
				}*/

					data.resultsRow [0].resultsColumn.Add (splitData [cur]);
				}
				return data;
			} else
				return null;
		}

		//Deserialize multiple JSON > ResultsIOData
		ResultsIOData FromJSON (string[] inputData)
		{
			if (inputData.Length != 0) {
				string[] separators = new string[]{ "\",\"", "\":\"" };//split by the two JSON separators
				ResultsIOData data = new ResultsIOData ();//new ResultsIOData

				for (int a = 0; a < inputData.Length; a++) {
					string[] splitData = inputData[a].Substring (2, inputData[a].Length - 5).Split (separators, System.StringSplitOptions.None);//remove curly brackets
					ResultsDataCommon RDC = new ResultsDataCommon ();
					for (int i = 0; i < splitData.Length; i++) {
						data.resultsRow.Add(new ResultsIORow());
						int cur = i;
						/*if (i == 18) {//inject the common class reference point - TODO remove if works
							data.resultsRow [i].resultsColumn.Add ("common");
							data.resultsRow [i].resultsColumn.Add ("GraphicsTestFramework.ResultsDataCommon");
						}*/
						data.resultsRow [a].resultsColumn.Add (splitData [cur]);

						switch (i) {
						case 1:
							RDC.DateTime = splitData [cur];
							break;
						case 3:
							RDC.UnityVersion = splitData [cur];
							break;
						case 5:
							RDC.AppVersion = splitData [cur];
							break;
						case 7:
							RDC.Platform = splitData [cur];
							break;
						case 9:
							RDC.API = splitData [cur];
							break;
						case 11:
							RDC.RenderPipe = splitData [cur];
							break;
						case 13:
							RDC.SceneName = splitData [cur];
							break;
						case 15:
							RDC.TestName = splitData [cur];
							break;
						default:
							break;
						}
					}
					data.resultsRow [a].commonResultsIOData = RDC;
				}
				return data;
			} else
				return null;
		}

		//Serialize ResultsIOData > JSON
		string[] ToJSON (ResultsIOData inputData)
		{
			if (inputData.resultsRow.Count <= 1) {
				Debug.LogWarning ("Invalid ResultsIOData sent to Serializer");
				return null;
			} else {
				string[] output = new string[inputData.resultsRow.Count];
				string[] tableLabels = new string[inputData.resultsRow [0].resultsColumn.Count];
				for (int i = 0; i < tableLabels.Length; i++)// column labels in string array
				tableLabels [i] = inputData.resultsRow [0].resultsColumn [i];
				for (int rows = 0; rows < inputData.resultsRow.Count; rows++) {
					output [rows] = "{";
					for (int columns = 0; columns < inputData.resultsRow [rows].resultsColumn.Count; columns++) {
						/*if (columns == 9)//rip out the commonResults class - TODO remove if works
							continue;*/
						string s = "\"" + inputData.resultsRow [0].resultsColumn [columns] + "\":\"" + inputData.resultsRow [rows].resultsColumn [columns] + "\"";
						if (columns < inputData.resultsRow [0].resultsColumn.Count - 1)
							s += ",";
						output [rows] += s;
					}
					output [rows] += "}";
				}
				if (Master.Instance.debugMode == Master.DebugMode.Messages)
					Debug.Log ("Serialized ResultsIOData");
				return output;
			}
		}

		//Serialize ResultsIOData > JSON
		string ToJSON (Dictionary<string, string> inputData)
		{
			string output = "{";
			int i = 0;
			foreach (string key in inputData.Keys) {
				string s = "\"" + key + "\":\"" + inputData[key] + "\"";
				if (i < inputData.Count - 1)
					s += ",";
				output += s;
				i++;
			}
			output += "}";
			return output;
		}

		//Getspace used by local files
		long CheckDiskSpace (string dir)
		{
			long bytes = new long ();
			DirectoryInfo dirInfo = new DirectoryInfo (dir);
			FileInfo[] fileInfo = dirInfo.GetFiles ();
			DirectoryInfo[] dirInfos = dirInfo.GetDirectories ();
			foreach (FileInfo fi in fileInfo) {
				bytes += fi.Length;
			}
			foreach (DirectoryInfo di in dirInfos) {
				bytes += CheckDiskSpace (di.FullName);
			}
			return bytes;
		}

		public static event Broadcast.EndResultsSave endResultsSave;

		public void BroadcastEndResultsSave ()
		{
			if (endResultsSave != null)
				endResultsSave ();
		}

		public static event Broadcast.LocalBaselineParsed baselinesParsed;

		public void BroadcastBaselineParsed ()
		{
			if (baselinesParsed != null)
			{
				baselinesParsed ();
			}
		}

	}

	/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------
	/// Data classes to recieve results from tests
	/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------
	[System.Serializable]
	public class ResultsIOData
	{
		public List<ResultsIORow> resultsRow = new List<ResultsIORow> ();
		//List of rows of results, row [0] is always the labels for the columns
	}

	[System.Serializable]
	public class ResultsIORow
	{
		public ResultsDataCommon commonResultsIOData = new ResultsDataCommon ();
		public List<string> resultsColumn = new List<string> ();
		//List of the actual results along the row, unless index [0] in results data then the labels
	}

	[System.Serializable]
	public class SuiteBaselineData
	{
		public string suiteName;
		public string platform;
		public string api;
		public string pipeline;
		public string suiteTimestamp;
		public List<SuiteData> _suiteData = new List<SuiteData>();
		//Suite information about baselines
	}

	[System.Serializable]
	public class SuiteData
	{
		public string testType;
		public string sceneName;
		public string testName;
		public string timeStamp;

		public SuiteData(string type, string scene, string name, string time){
			testType = type;
			sceneName = scene;
			testName = name;
			timeStamp = time;
		}
	}

	[System.Serializable]
	public enum fileType
	{
		Result,
		Baseline,
		SuiteData,
		BaseData}
	;

}