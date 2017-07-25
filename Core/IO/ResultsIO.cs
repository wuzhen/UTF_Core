using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace GraphicsTestFramework
{
	public class ResultsIO : MonoBehaviour
	{
		private static ResultsIO _Instance = null;
		private List<string> suiteBaselinesPullList = new List<string> ();
		private SystemData sysData;
		public bool isWaiting = false;
		public bool companionMode = false;
		public bool writeLocal = true;
		public bool writeCloud = true;

		//List of suiteBaselineData for suites
		public List<SuiteBaselineData> _suiteBaselineData = new List<SuiteBaselineData> ();
		//TODO make private once working

		public static ResultsIO Instance {
			get {
				if (_Instance == null)
					_Instance = (ResultsIO)FindObjectOfType (typeof(ResultsIO));
				return _Instance;
			}
		}

		private void Start ()
		{
			sysData = Master.Instance.GetSystemData ();

			//setup local IO
			if (LocalIO.Instance == null)
				gameObject.AddComponent<LocalIO> ();
			LocalIO.Instance.Init (sysData);
			//setup cloud IO
			if (CloudIO.Instance == null)
				gameObject.AddComponent<CloudIO> ();
			CloudIO.Instance.Init (sysData);

			if (!companionMode)
				StartCoroutine (Init ());
		}

		public void Restart ()
		{
			StartCoroutine (Init ());
		}

		public IEnumerator Init ()
		{
			//Show loading screen
			ProgressScreen.Instance.SetState (true, ProgressType.LocalLoad, "Loading local data");
			_suiteBaselineData = LocalIO.Instance.ReadLocalBaselines ();// - TODO this needs to get called again at some point

			//Hardcoded wait for SuiteManager to populate - TODO might be cleaner way to do later
			float timeout = 0;
			while (SuiteManager.GetSuiteNames ().Length == 0) {
				timeout += Time.deltaTime;
				if (timeout > 5f) {
					break;
				}
				yield return new WaitForEndOfFrame ();
			}

			//fetch suite names from the suite manager
			string[] suiteNames = SuiteManager.GetSuiteNames ();
			if (suiteNames.Length == 0) {
				Console.Instance.Write (DebugLevel.Critical, MessageLevel.LogWarning, "No suites loaded in SuiteManager, unable to continue"); // Write to console
			} else {
				foreach (string suiteName in suiteNames) {
					CloudIO.Instance.GetBaselineTimestamp (suiteName);
					while (CloudConnectorCore.isWaiting == true) {
						yield return new WaitForEndOfFrame ();
					}
				}

				yield return new WaitForSeconds (1f);

				if (suiteBaselinesPullList.Count > 0)
					CloudIO.Instance.FetchCloudBaselines (suiteBaselinesPullList.ToArray ());
				else {
					Console.Instance.Write (DebugLevel.Logic, MessageLevel.Log, "No cloud based baselines to pull"); // Write to console
					BroadcastBaselineParsed ();
				}
			}
		}

		//cloud data recieved from CloudIO parsed by LocalIO and sent here
		public void CloudBaselineDataRecieved (List<SuiteBaselineData> LocalBaselines)
		{
			_suiteBaselineData = LocalBaselines;
			BroadcastBaselineParsed ();
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
		public bool BaselineExists (string suiteName, string pipeline, string testType, string sceneName, string testName)
		{
			//check local file for current platform/api baseline
			foreach (SuiteBaselineData SBD in _suiteBaselineData) {
				if (SBD.suiteName == suiteName) {
                    //Suite exists
                    if (SBD.platform == sysData.Platform)
                    {
                        //Platform exists
                        if (SBD.api == sysData.API)
                        {
                            //API matches
                            if (SBD.pipeline == pipeline)
                            {
                                //pipeline exists
                                foreach (SuiteData SD in SBD._suiteData)
                                {
                                    if (SD.testType == testType && SD.sceneName == sceneName && SD.testName == testName)
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
				}
			}
			return false;
		}

		void CompareBaselineTimestamps (string suiteName, string dateTime)
		{
			System.DateTime cloudTimestamp = System.DateTime.Parse (dateTime);

			if (_suiteBaselineData.Count == 0) {//TODO - shouldnt add this to pull baselines as has issue with iOS trying to pull baselines for OSX
				Console.Instance.Write (DebugLevel.Full, MessageLevel.Log, "Putting " + suiteName + " in the pull list"); // Write to console
				suiteBaselinesPullList.Add (suiteName);
			} else {
				int matches = 0;
				foreach (SuiteBaselineData SBD in _suiteBaselineData) {
					if (SBD.suiteName == suiteName && SBD.platform == sysData.Platform && SBD.api == sysData.API) {
						matches++;
						System.DateTime localTimestamp = System.DateTime.Parse (SBD.suiteTimestamp);

						int timeDiff = cloudTimestamp.CompareTo (localTimestamp);
						if (timeDiff < 0f) {
							Console.Instance.Write (DebugLevel.Full, MessageLevel.Log, "Cloud Timestamp is old"); // Write to console
						} else if (timeDiff > 0f) {
							Console.Instance.Write (DebugLevel.Full, MessageLevel.Log, "Cloud Timestamp is newer, adding " + suiteName + " to pull list"); // Write to console
							suiteBaselinesPullList.Add (suiteName);
						} else if (timeDiff == 0f) {
							Console.Instance.Write (DebugLevel.Full, MessageLevel.Log, "Cloud Timestamp is the same"); // Write to console
						}
					}
				}

				if (matches == 0)
					suiteBaselinesPullList.Add (suiteName);

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
			ProgressScreen.Instance.SetState (true, ProgressType.CloudSave, "Saving local and cloud data");

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
				StartCoroutine (CloudIO.Instance.UploadData (data, sheetName, baseline, inputData.resultsRow [0].resultsColumn.ToArray ()));
				;
			}

			if (data != null) {
				inputData.resultsRow.RemoveAt (0);

				for (int i = 0; i < data.Length - 1; i++) {
					data [i] = data [i + 1];
				}
				Array.Resize<string> (ref data, data.Length - 1);
				StartCoroutine (LocalIO.Instance.WriteDataFiles (suiteName, testType, inputData, data, ft));
			} else {
				Console.Instance.Write (DebugLevel.Critical, MessageLevel.LogWarning, "Results are empty for Suite: " + suiteName + " Type: " + testType + ". Nothing to write"); // Write to console
				BroadcastEndResultsSave ();
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
		public ResultsIOData RetrieveBaseline (string suiteName, string testType, ResultsDataCommon inputData)
		{
			string rawJSONdata = LocalIO.Instance.FetchDataFile (suiteName, testType, inputData, true);//fetch string from file
			ResultsIOData data = JSONHelper.FromJSON (rawJSONdata);//take JSON convert to ResultsIOData //REORG
			return data;
		}

		/// <summary>
		/// Retrieves a result file.
		/// </summary>
		/// <returns>The result.</returns>
		/// <param name="suiteName">Suite name.</param>
		/// <param name="testType">Test type.</param>
		/// <param name="inputData">Input data.</param>
		public void RetrieveResults (string suiteName, string testType)
		{
			CloudIO.Instance.FetchCloudResults (suiteName, testType);
		}

		/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		/// In memory suite list
		/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		public void ProcessBaselineTimestamp (List<string> objNames, List<string> jsonData)
		{ // TODO- externalize JSON splitting for multiple json arrays from cloudconnector
			if (jsonData [0].Length > 2) {
				jsonData [0] = jsonData [0].Remove (0, 1);
				jsonData [0] = jsonData [0].Remove (jsonData [0].Length - 1, 1);
				string[] separators = new string[] { "},{" };
				string[] splitJson = jsonData [0].Split (separators, System.StringSplitOptions.None);


				for (int i = 0; i < splitJson.Length; i++) {
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
					if (jsonDic ["api"] == sysData.API && jsonDic ["platform"] == sysData.Platform) {
						CompareBaselineTimestamps (jsonDic ["suiteName"], jsonDic ["suiteTimestamp"]);
					}
				}

			}
			isWaiting = false;
		}

		/// <summary>
		/// Updates the baseline dictionary(base not individual baselines, call this first then BaselineDictionaryEntry).
		/// This has an out of an int for knowing what index the particular suite is located in the list
		/// </summary>
		/// <param name="suite">Suite.</param>
		/// <param name="renderPipe">Render pipe.</param>
		/// <param name="baselineIndex">Baseline index in the list.</param>
		public void UpdateBaselineDictionary (string suite, string renderPipe, out int baselineIndex)
		{
			//Updates suiteListData, we need to find the matching suiteListData or create a new one
			int suiteBaselineDataIndex = -1;
			int suiteIndex = 0;
			foreach (SuiteBaselineData SBD in _suiteBaselineData) {
				if (SBD.platform == sysData.Platform && SBD.suiteName == suite && SBD.api == sysData.API && SBD.pipeline == renderPipe) {
					suiteBaselineDataIndex = suiteIndex;
				}
				suiteIndex++;
			}
			if (suiteBaselineDataIndex == -1) {
				SuiteBaselineData newSBD = new SuiteBaselineData ();
				newSBD.api = sysData.API;
				newSBD.pipeline = renderPipe;
				newSBD.platform = sysData.Platform;
				newSBD.suiteName = suite;
				newSBD.suiteTimestamp = System.DateTime.UtcNow.ToString ();
				_suiteBaselineData.Add (newSBD);
				suiteBaselineDataIndex = _suiteBaselineData.Count - 1;
			}
			_suiteBaselineData [suiteBaselineDataIndex].suiteTimestamp = System.DateTime.UtcNow.ToString ();
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
		public void BaselineDictionaryEntry (int suiteIndex, string testType, string sceneName, string testName, string dateTime)
		{

			SuiteData sData = new SuiteData (testType, sceneName, testName, dateTime);
			int check = -1;
			for (int si = 0; si < _suiteBaselineData [suiteIndex]._suiteData.Count; si++) {
				SuiteData SD = _suiteBaselineData [suiteIndex]._suiteData [si];
				if (SD.testType == testType && SD.sceneName == sceneName && SD.testName == testName) {
					_suiteBaselineData [suiteIndex]._suiteData [si] = sData;
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
				baselinesParsed ();
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
		public List<SuiteData> _suiteData = new List<SuiteData> ();
		//Suite information about baselines
	}

	[System.Serializable]
	public class SuiteData
	{
		public string testType;
		public string sceneName;
		public string testName;
		public string timeStamp;

		public SuiteData (string type, string scene, string name, string time)
		{
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