using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphicsTestFramework
{
	public class CloudIO : MonoBehaviour
	{
		public static CloudIO _Instance = null;

		public static CloudIO Instance {
			get {
				if (_Instance == null)
					_Instance = (CloudIO)FindObjectOfType (typeof(CloudIO));
				return _Instance;
			}
		}

		private SystemData sysData;
		//Whether in editor or not for cloud connector
		private bool cloudMode;

		public string cloudResponse = null;

		//Setup cloud listners
		void OnEnable ()
		{
			// Suscribe for catching cloud responses.
			CloudConnectorCore.processedResponseCallback.AddListener (ParseCloudData);
			CloudImagesConnector.Instance.responseCallback.AddListener (ParseData);
		}

		void OnDisable ()
		{
			// Remove listeners.
			CloudConnectorCore.processedResponseCallback.RemoveListener (ParseCloudData);
			CloudImagesConnector.Instance.responseCallback.RemoveListener (ParseData);

		}

		public void Init (SystemData systemData)
		{
			#if UNITY_EDITOR
			cloudMode = true;
			#else
			cloudMode = false;
			#endif

			sysData = systemData;
		}

		/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		/// Sending data - TODO wip
		/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Uploads the data.
		/// Send results with baseline switch TODO - need to make it so baseline files override if exist otherwise add(maybe new cloudconnector function)
		/// </summary>
		/// <returns>The data.</returns>
		/// <param name="jsonData">Json data array</param>
		/// <param name="sheetName">Sheet name.</param>
		/// <param name="baseline">Baseline.</param>
		public IEnumerator UploadData (string[] jsonData, string sheetName, int baseline, string[] fields)
		{
			Console.Instance.Write (DebugLevel.Full, MessageLevel.Log, "Uploading JSON data to sheet=" + sheetName); // Write to console
			float uploadStartTime = Time.realtimeSinceStartup;

			string[] trimmedJson = new string[jsonData.Length - 1];
			for (int i = 1; i < jsonData.Length; i++) {
				trimmedJson [i - 1] = jsonData [i];
			}
			CreateWorksheet (fields, sheetName);
			//wait for cloudcore to be idle
			while (CloudConnectorCore.isWaiting)
				yield return null;

			if (baseline == 1) {
				//replace each individually
				for (int i = 0; i < trimmedJson.Length; i++) {
					//update rows based on matching columns 6, 7, 8, 9, 10 > Platform, API, RenderPipe, SceneName, TestName
					CloudConnectorCore.UpdateUniqueRow (sheetName, new int[]{ 6, 7, 8, 9, 10 }, trimmedJson [i], cloudMode);
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
			Console.Instance.Write (DebugLevel.Full, MessageLevel.Log, "Upload of " + (jsonData.Length - 1) + " items in " + (Time.realtimeSinceStartup - uploadStartTime) + "ms"); // Write to console
			while (CloudConnectorCore.isWaiting || CloudImagesConnector.responseCount != 0)
				yield return new WaitForEndOfFrame ();
			ResultsIO.Instance.BroadcastEndResultsSave ();
		}

		/// <summary>
		/// Checks the worksheet exists.
		/// </summary>
		/// <param name="sheetName">Sheet name.</param>
		public void CheckWorksheet (string sheetName)
		{
			CloudConnectorCore.TableExists (sheetName, cloudMode);
		}

		/// <summary>
		/// Creates the worksheet.
		/// </summary>
		/// <param name="headers">Worksheet headers.</param>
		/// <param name="sheetName">Sheet name.</param>
		void CreateWorksheet (string[] headers, string sheetName)
		{
			CloudConnectorCore.CreateTable (headers, sheetName, cloudMode);
		}

		//Set baseline timestamp
		public void SetBaselineTimestamp (SuiteBaselineData SBD)
		{
			//update suite timestamp and baseline timestamp
			//string[] headers = new string[]{"suiteName", "platform", "api", "suiteTimestamp"};
			//CloudConnectorCore.CreateTable (headers, "SuiteBaselineTimestamps", cloudMode);// TODO - may never need again but should resolve
			Dictionary<string, string> entry = new Dictionary<string, string> ();

			entry ["suiteName"] = SBD.suiteName;
			entry ["platform"] = SBD.platform;
			entry ["api"] = SBD.api;
			entry ["suiteTimestamp"] = SBD.suiteTimestamp;

			CloudConnectorCore.UpdateUniqueRow ("SuiteBaselineTimestamps", new int[]{ 1, 2, 3 }, JSONHelper.ToJSON (entry), cloudMode);//REORG
		}

		/// <summary>
		/// Sends a large entry to the EXTERNAL_DATA cloud storage.
		/// </summary>
		/// <returns>The large entry.</returns>
		/// <param name="value">Value.</param>
		/// <param name="key">Key.</param>
		public string ConvertLargeEntry (string value, string key)
		{
			string UID = "REPLACEMENT_" + CoreUtilities.StringToUI (key);
			CloudImagesConnector.PersistText (value, UID);
			return UID;
		}

		/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		/// Requesting data - TODO wip
		/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Pull relevant baseline data from cloud.
		/// </summary>
		public void FetchCloudBaselines (string[] suiteNames)
		{
			ProgressScreen.Instance.SetState (true, ProgressType.CloudLoad, "Retrieving cloud data");
			Console.Instance.Write (DebugLevel.Full, MessageLevel.Log, "getting baselines for " + sysData.Platform + " on API " + sysData.API); // Write to console
			foreach (string s in suiteNames)
				Console.Instance.Write (DebugLevel.Full, MessageLevel.Log, "pulling suite=" + s); // Write to console
			CloudConnectorCore.GetBaselineData (suiteNames, sysData.Platform, sysData.API, cloudMode);
		}

		/// <summary>
		/// Gets the baseline timestamp from the cloud.
		/// </summary>
		/// <param name="suiteName">Suite name.</param>
		public void GetBaselineTimestamp (string suiteName)
		{
			ProgressScreen.Instance.SetState (true, ProgressType.CloudLoad, "Checking for inconsistencies with cloud data");
			CloudConnectorCore.GetObjectsByField ("SuiteBaselineTimestamps", "suiteName", suiteName, cloudMode);
		}

		/// <summary>
		/// Pull Entire sheet.
		/// </summary>
		public void FetchCloudResults (string suiteName, string testType)
		{
			string tableName = suiteName + "_" + testType + "_Results";
			CloudConnectorCore.GetTable (tableName, cloudMode);
		}

		/// <summary>
		/// Pull relevant baseline data from cloud.
		/// </summary>
		public void FetchCloudResults (string suiteName, string testType, ResultsDataCommon commonData)
		{
			string tableName = suiteName + "_" + testType + "_Results";

		}

		/// <summary>
		/// Fetchs the large entry.
		/// </summary>
		/// <returns>the originnal string.</returns>
		/// <param name="UID">The key for the file.</param>
		public void FetchLargeEntry (string UID)
		{
			string output = UID.Replace ("\"", ""); // TODO - Look at this
			CloudImagesConnector.RequestTxt (output);
		}

		/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		/// Recieveing data - TODO wip
		/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Parses the cloud data.
		/// </summary>
		public void ParseCloudData (CloudConnectorCore.QueryType query, List<string> objTypeNames, List<string> jsonData)
		{

			if (query == CloudConnectorCore.QueryType.tableExists) {
				Console.Instance.Write (DebugLevel.Full, MessageLevel.Log, jsonData [0]); // Write to console
			}

			if (query == CloudConnectorCore.QueryType.getBaselineData) {
				StartCoroutine (LocalIO.Instance.CreateLocalFromCloud (objTypeNames, jsonData));
			}

			if (query == CloudConnectorCore.QueryType.getObjects) {
				if (objTypeNames [0] == "SuiteBaselineTimestamps") {
					if (jsonData [0].Length < 4)
						Console.Instance.Write (DebugLevel.Key, MessageLevel.LogWarning, "Baseline timestamp does not exist in cloud"); // Write to console
					else
						ResultsIO.Instance.ProcessBaselineTimestamp (objTypeNames, jsonData);
				}
			}

			if (query == CloudConnectorCore.QueryType.baselineNone) {
				ResultsIO.Instance.BroadcastBaselineParsed ();
			}

		}

		/// <summary>
		/// Parses the data from Cloud image connector.
		/// </summary>
		void ParseData (string responseType, string response)
		{
			if (responseType == "DATA_") {
				//process cloud data coming down
				string value = response.Split (new string[]{ "_FILE_NAME_" }, System.StringSplitOptions.None) [1];
				string name = response.Remove (response.IndexOf ("_FILE_NAME_")).TrimEnd ((".png").ToCharArray ());
				LocalIO.Instance.LargeFileWrite (value, name);
			}
		}

		/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		/// Utilities - TODO wip
		/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------


	}
}
