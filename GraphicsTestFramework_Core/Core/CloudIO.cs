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

		//Whether in editor or not for cloud connector
		private bool cloudMode;

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

		private void Start ()
		{
			#if UNITY_EDITOR
			cloudMode = true;
			#else
			cloudMode = false;
			#endif
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
		public IEnumerator UploadData(string[] jsonData, string sheetName, int baseline, string[] fields){
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

			CloudConnectorCore.UpdateUniqueRow ("SuiteBaselineTimestamps", new int[]{1, 2, 3},  JSONHelper.ToJSON (entry), cloudMode);//REORG
		}

		/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		/// Requesting data - TODO wip
		/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Pull relevant baseline data from cloud.
		/// </summary>
		public void FetchCloudBaselines(string[] suiteNames){
			ProgressScreen.Instance.SetState(true, ProgressType.CloudLoad, "Retrieving cloud data");
			SystemData sysData = Master.Instance.GetSystemData ();
			Debug.Log ("getting baselines for " + sysData.Platform + " on API " + sysData.API);
			foreach (string s in suiteNames)
				Debug.Log ("pulling suite=" + s);
			CloudConnectorCore.GetBaselineData (suiteNames, sysData.Platform, sysData.API, cloudMode);
		}

		/// <summary>
		/// Gets the baseline timestamp from the cloud.
		/// </summary>
		/// <param name="suiteName">Suite name.</param>
		public void GetBaselineTimestamp(string suiteName){
			ProgressScreen.Instance.SetState(true, ProgressType.CloudLoad, "Checking for inconsistencies with cloud data");
			CloudConnectorCore.GetObjectsByField ("SuiteBaselineTimestamps", "suiteName", suiteName, cloudMode);
		}

		/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		/// Recieveing data - TODO wip
		/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Parses the cloud data.
		/// </summary>
		/// <param name="query">Query type</param>
		/// <param name="objTypeNames">Object type names.</param>
		/// <param name="jsonData">Json data.</param>
		public void ParseCloudData(CloudConnectorCore.QueryType query, List<string> objTypeNames, List<string> jsonData){

			if (query == CloudConnectorCore.QueryType.tableExists) {
				Debug.Log (jsonData [0]);
			}

			if (query == CloudConnectorCore.QueryType.getBaselineData) {
				StartCoroutine (LocalIO.Instance.CreateLocalFromCloud (objTypeNames, jsonData));
			}

			if(query == CloudConnectorCore.QueryType.getObjects){
				if (objTypeNames [0] == "SuiteBaselineTimestamps"){
					if(jsonData[0].Length < 4)
						Debug.LogWarning("Baseline timestamp does not exist in cloud");
					else
						ResultsIO.Instance.ProcessBaselineTimestamp (objTypeNames, jsonData);
				}
			}

		}

		/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		/// Utilities - TODO wip
		/// ------------------------------------------------------------------------------------------------------------------------------------------------------------------------


    }
}
