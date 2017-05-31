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
		//public long spaceUsed;>>>>MOVED TO LOCAL IO
		//private string dataPath;>>>>MOVED TO LOCAL IO
		//private string baselinePrefix = "Baseline";>>>>MOVED TO LOCAL IO
		//private string resultsCurrentPrefix = "ResultsCurrent";>>>>MOVED TO LOCAL IO
		private List<string> suiteBaselinesPullList = new List<string>();
		public bool isWaiting = false;

		//cloud specific variables
		//private bool cloudMode;>>>>>MOVED TO CLOUD IO

		//List of suiteBaselineData for suites
		public List<SuiteBaselineData> _suiteBaselineData = new List<SuiteBaselineData>(); //TODO make private once working

		public static ResultsIO Instance {
			get {
				if (_Instance == null)
					_Instance = (ResultsIO)FindObjectOfType (typeof(ResultsIO));
				return _Instance;
			}
		}

		private IEnumerator Start ()
		{
            yield return new WaitForEndOfFrame(); // TODO - Refactor this
			//Show loading screen
			ProgressScreen.Instance.SetState(true, ProgressType.LocalLoad, "Loading local data");
			_suiteBaselineData = LocalIO.Instance.ReadLocalBaselines ();// - TODO this needs to get called again at some point

			//Hardcoded wait for SuiteManager to populate - TODO might be cleaner way to do later
			float timeout = 0;
			while(SuiteManager.Instance.GetSuiteNames().Length == 0){
				timeout += Time.deltaTime;
				if(timeout > 5f){
					break;
				}
				yield return new WaitForEndOfFrame();
			}

			//fetch suite names from the suite manager
			string[] suiteNames = SuiteManager.Instance.GetSuiteNames ();
			if(suiteNames.Length == 0){
				Debug.LogWarning("No suites loaded in SuiteManager, unable to continue");
			}else{
				foreach(string suiteName in suiteNames){
					CloudIO.Instance.GetBaselineTimestamp (suiteName);
					while(CloudConnectorCore.isWaiting == true){
						yield return new WaitForEndOfFrame ();
					}
				}

				yield return new WaitForSeconds(1f);

				if(suiteBaselinesPullList.Count > 0)
					CloudIO.Instance.FetchCloudBaselines (suiteBaselinesPullList.ToArray ());
				else{
					Debug.LogWarning("No cloud based baselines to pull");
					BroadcastBaselineParsed ();
				}
			}
		}

		//cloud data recieved from CloudIO parsed by LocalIO and sent here
		public void CloudBaselineDataRecieved(List<SuiteBaselineData> LocalBaselines){
			_suiteBaselineData = LocalBaselines;
			BroadcastBaselineParsed();
		}


		/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		/// Initial checks
		/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------

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

			if(_suiteBaselineData.Count == 0){//TODO - shouldnt add this to pull baselines as has issue with iOS trying to pull baselines for OSX
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
			string[] data = JSONHelper.ToJSON (inputData);//REORG

			fileType ft;
			if (baseline == 1) {
				ft = fileType.Baseline;
				//Cloud upload for baseline
				string sheetName = suiteName + "_" + testType + "_Baseline";

				StartCoroutine (CloudIO.Instance.UploadData (data, sheetName, baseline, inputData.resultsRow [0].resultsColumn.ToArray ()));
			} else {
				ft = fileType.Result;
				//cloud upload for results
				string sheetName = suiteName + "_" + testType + "_Results";
				StartCoroutine (CloudIO.Instance.UploadData (data, sheetName, baseline, inputData.resultsRow [0].resultsColumn.ToArray ()));;
			}

			if (data != null) {
				inputData.resultsRow.RemoveAt (0);

				for(int i = 0; i < data.Length - 1; i++){
					data [i] = data [i + 1];
				}
				Array.Resize<string>(ref data, data.Length-1);
				StartCoroutine (LocalIO.Instance.WriteDataFiles (suiteName, testType, inputData, data, ft));
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
			string rawJSONdata = LocalIO.Instance.FetchDataFile (suiteName, testType, inputData, false);//fetch string from file
			ResultsIOData data = JSONHelper.FromJSON (rawJSONdata);//take JSON convert to ResultsIOData //REORG
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
			string rawJSONdata = LocalIO.Instance.FetchDataFile (suiteName, testType, inputData, true);//fetch string from file
			ResultsIOData data = JSONHelper.FromJSON (rawJSONdata);//take JSON convert to ResultsIOData //REORG
			return data;
		}

		/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		/// In memory suite list
		/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		public void ProcessBaselineTimestamp(List<string> objNames, List<string> jsonData){ // TODO- externalize JSON splitting for multiple json arrays from cloudconnector
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
					Dictionary<string, string> jsonDic = JSONHelper.JSON_Dictionary (jsonString);//REORG
					if(jsonDic["api"] == Master.Instance.GetSystemData ().API){
						CompareBaselineTimestamps (jsonDic["suiteName"], jsonDic["suiteTimestamp"]);
					}
				}

			}isWaiting = false;
		}

		/// <summary>
		/// Updates the baseline dictionary(base not individual baselines, call this first then BaselineDictionaryEntry).
		/// This has an out of an int for knowing what index the particular suite is located in the list
		/// </summary>
		/// <param name="suite">Suite.</param>
		/// <param name="renderPipe">Render pipe.</param>
		/// <param name="baselineIndex">Baseline index in the list.</param>
		public void UpdateBaselineDictionary(string suite, string renderPipe, out int baselineIndex){
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
		public void BaselineDictionaryEntry(int suiteIndex, string testType, string sceneName, string testName, string dateTime){
			
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

		/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		/// LISTENERS
		/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------


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