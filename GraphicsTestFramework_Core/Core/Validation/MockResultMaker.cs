using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Reflection;
using System.IO;
using System;

namespace GraphicsTestFramework
{
	public class MockResultMaker : MonoBehaviour
	{
        /*
		public ResultsDataCommon commonData;
		public string testType;
		public string testSuiteName;
		public int testNum = 0;
		//BaselineTemp list
		public List<ExampleLogic.ResultsData> baseline = new List<ExampleLogic.ResultsData> ();
		//List of results collected
		public List<ExampleLogic.ResultsData> results = new List<ExampleLogic.ResultsData> ();
		//List of baselines loaded
		public List<ExampleLogic.ResultsData> loadedBaselines = new List<ExampleLogic.ResultsData> ();

		public ResultsDataCommon testBaselineFetch;

        private void Start()
        {
            ExampleLogic.ResultsData x = GenerateDummyData("0");
            commonData = x.common;
        }

		void Update ()
		{
			if (Input.GetKeyUp(KeyCode.Space)) { //dummy data gen
				results.Add (GenerateDummyData (testNum.ToString ()));
				testNum++;//adds so test name is different per added test results
			}

			if (Input.GetKeyUp(KeyCode.Return)) { //sendDummy results data to ResultsIO
				SendDataToResultsIO (0);
			}

			if (Input.GetKeyUp(KeyCode.B)) { //sendDummy baseline data to ResultsIO
				//baseline.Add (GenerateDummyData (testNum.ToString ()));
				//testNum++;//adds so test name is different per added test results
				//results = baseline;
				SendDataToResultsIO (1);
			}

			if (Input.GetKeyUp(KeyCode.R)) {
				Directory.Delete ((Application.dataPath).Substring (0, Application.dataPath.Length - 6) + "EditorResults/" + testSuiteName, true);
			}

			if(Input.GetKeyUp(KeyCode.F)){
				loadedBaselines.Add (GenerateBaselineData(ResultsIO.Instance.RetrieveBaseline(testSuiteName, testType, commonData)));
				//foreach(ExampleLogic.ResultsData rd in baseline){
				//	GenerateBaselineData(ResultsIO.Instance.RetrieveResult(testSuiteName, testType, rd.common));
				//}
			}

			if(Input.GetKeyUp (KeyCode.T)){
				ResultsIO.Instance.BaselineExists (testSuiteName, testBaselineFetch.RenderPipe, testType, testBaselineFetch.SceneName, testBaselineFetch.TestName);
			}

			//if(Input.GetKeyUp (KeyCode.C)){
			//	ResultsIO.Instance.CheckWorksheet (testSuiteName);
			//}
		}

		ExampleLogic.ResultsData GenerateDummyData (string testName)
		{
			ExampleLogic.ResultsData resultsData = new ExampleLogic.ResultsData ();
			resultsData.common = new ResultsDataCommon ();
			SystemData sysData = Master.Instance.GetSystemData ();
			resultsData.common.AppVersion = sysData.AppVersion;
			resultsData.common.API = sysData.API;
			resultsData.common.DateTime = Master.Instance.GetSystemTime ().ToString ();
			resultsData.common.PassFail = commonData.PassFail;
			resultsData.common.Platform = sysData.Platform;
			resultsData.common.RenderPipe = commonData.RenderPipe;
			resultsData.common.UnityVersion = sysData.UnityVersion;
			resultsData.common.SceneName = SceneManager.GetActiveScene ().name;
			resultsData.common.TestName = "TestData" + testName;

			//resultsData.SomeString = "T|e|s|t| |S|t|r|i|n|g";
			resultsData.SomeFloat = UnityEngine.Random.value;
			resultsData.SomeInt = Mathf.RoundToInt (resultsData.SomeFloat);
            //resultsData.SomeStringArray = new string[2] { "Entry1", "Entry2" };
			//if (resultsData.SomeFloat > 0.5f)
			//	resultsData.SomeBool = true;
			//else
			//	resultsData.SomeBool = false;

			return resultsData;
		}

		/// ------------------------------------------------------------------------------------
		/// Below here should drop right into testlogic.

		//Send results data to ResultsIO
		void SendDataToResultsIO (int baseline)
		{
			ResultsIOData output = GenerateResultsData ();
			if (Master.Instance.debugMode == Master.DebugMode.Messages)
				Debug.Log (this.GetType ().Name + " is sending data to ResultsIO");
			ResultsIO.Instance.ProcessResults (testSuiteName, testType, output, baseline);
			results.Clear ();
		}

		//Serialize ResultsData(class) to ResultsIOData(string arrays)
		ResultsIOData GenerateResultsData ()
		{
			ResultsIOData output = new ResultsIOData ();
			for (int r = 0; r < results.Count + 1; r++)
				output.resultsRow.Add (new ResultsIORow ());
			BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
			FieldInfo[] commonFields = typeof(ResultsDataCommon).GetFields (bindingFlags);
			FieldInfo[] customFields = typeof(ExampleLogic.ResultsData).GetFields (bindingFlags);
			output.resultsRow [0] = new ResultsIORow ();
			for (int f = 0; f < commonFields.Length; f++)
				output.resultsRow [0].resultsColumn.Add (commonFields [f].Name);
			for (int f = 0; f < customFields.Length; f++)
				output.resultsRow [0].resultsColumn.Add (customFields [f].Name);
			for (int r = 0; r < results.Count; r++) {
				output.resultsRow [r + 1].commonResultsIOData = results [r].common;
				output.resultsRow [r + 1].resultsColumn = new List<string> ();
				for (int f = 0; f < commonFields.Length; f++) {
					var commonResult = results [0].common.GetType ().GetField (commonFields [f].Name).GetValue (results [r].common);
					output.resultsRow [r + 1].resultsColumn.Add (commonResult.ToString ());
				}
				for (int f = 0; f < customFields.Length; f++) {
					var customResult = results [0].GetType ().GetField (customFields [f].Name).GetValue (results [r]);
                    if (results[0].GetType().GetField(customFields[f].Name).FieldType.IsArray) // This handles arrays
                    {
                        Array a = (Array)results[0].GetType().GetField(customFields[f].Name).GetValue(results[r]);
                        string[] stringArray = new string[a.Length]; 
                        for (int i = 0; i < a.Length; i++)
                            stringArray[i] = a.GetValue(i).ToString();
                        customResult = Common.ConvertStringArrayToString(stringArray);
                    }
                    output.resultsRow [r + 1].resultsColumn.Add (customResult.ToString ());
				}
			}
			if (Master.Instance.debugMode == Master.DebugMode.Messages)
				Debug.Log (this.GetType ().Name + " generated resultsIO data");
			return output;
		}

        //ResultsIOData > ResultsData, using ExampleLogic but when embedded can be removed
        ExampleLogic.ResultsData GenerateBaselineData(ResultsIOData resultsIOData)
        {
            ExampleLogic.ResultsData resultData = new ExampleLogic.ResultsData();//blank results data
            resultData.common = new ResultsDataCommon();//blank common data

            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            FieldInfo[] commonFields = typeof(ResultsDataCommon).GetFields(bindingFlags);
            FieldInfo[] customFields = typeof(ExampleLogic.ResultsData).GetFields(bindingFlags);

            List<string> commonDataRaw = resultsIOData.resultsRow[0].resultsColumn.GetRange(0, commonFields.Length * 2);
            List<string> resultsDataRaw = resultsIOData.resultsRow[0].resultsColumn.GetRange(commonFields.Length * 2, resultsIOData.resultsRow[0].resultsColumn.Count - (commonFields.Length * 2));

            for (int f = 0; f < customFields.Length; f++)
            {
                if (f == 0)
                {
                    //do the common class
                    for (int cf = 0; cf < commonFields.Length; cf++)
                    {
                        string value = commonDataRaw[(cf * 2) + 1];
                        FieldInfo fieldInfo = resultData.common.GetType().GetField(commonFields[cf].Name);
                        fieldInfo.SetValue(resultData.common, Convert.ChangeType(value, fieldInfo.FieldType));
                    }
                }
                else
                {
                    var value = resultsDataRaw[(f * 2) + 1];
                    FieldInfo fieldInfo = resultData.GetType().GetField(customFields[f].Name);
                    if (fieldInfo.FieldType.IsArray) // This handles arrays
                    {
                        Type type = resultData.GetType().GetField(customFields[f].Name).FieldType.GetElementType();
                        GenerateGenericArray(fieldInfo, resultData.GetType(), resultData, type, value);
                    }
                    else // Non array types
                        fieldInfo.SetValue(resultData, Convert.ChangeType(value, fieldInfo.FieldType));
                }
            }
            return resultData;
        }

        public void GenerateGenericArray(FieldInfo fieldInfo, Type resultType, object resultObject, Type arrayType, string arrayValue)
        {
            var resultData = System.Convert.ChangeType(resultObject, resultType);
            string[] stringArray = arrayValue.Split(new string[1] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            Debug.Log("Generating Generic Array for " + resultData + " of type " + arrayType.ToString());
            switch (arrayType.ToString())
            {
                case "System.Object":
                    object[] objectArray = new object[stringArray.Length];
                    for (int i = 0; i < objectArray.Length; i++)
                        objectArray[i] = Convert.ChangeType(stringArray[i], arrayType);
                    fieldInfo.SetValue(resultData, objectArray);
                    break;
                case "System.String":
                    fieldInfo.SetValue(resultData, stringArray);
                    break;
            }
        }*/
    }
}
