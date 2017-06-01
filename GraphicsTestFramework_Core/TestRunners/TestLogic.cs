using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphicsTestFramework
{
    public abstract class TestLogicBase : MonoBehaviour
    {
        /// ------------------------------------------------------------------------------------
        /// State and type variables

        public enum StateType { CreateResults, CreateBaseline };
        public StateType stateType;
        public RunnerType activeRunType;

        /// ------------------------------------------------------------------------------------
        /// Test specific variables

        [HideInInspector] public string testTypeName;
		[HideInInspector] public string testSuiteName;
        [HideInInspector] public TestList.TestInfo activeTestInfo; //Common information about the active test
        [HideInInspector] public Type displayType;
        //[HideInInspector] public TestDisplayBase displayObject;

        /// ------------------------------------------------------------------------------------
        /// Results variables

        public List<object> results = new List<object>();
        public object baseline; //Baseline to compare to (cast to logic's result class)
        public object activeResultData; //Results data to write to (cast to logic's result class)
        public Type resultsType;
        ResultsDataCommon resultsCommonTemplate = new ResultsDataCommon(); //Used for reflection
        
        /// ------------------------------------------------------------------------------------
        /// Broadcast variables

        public static event Broadcast.EndTestAction endTestAction;
		public void BroadcastEndTestAction ()
		{
			if (endTestAction != null)
				endTestAction ();
		}

        /// ------------------------------------------------------------------------------------
        /// Broadcast delegate subscriptions

        //Subscribe to event delegates
        void OnEnable()
        {
            ResultsIO.endResultsSave += ConfirmResultsSaved;
        }

        //Desubscribe from event delegates
        void OnDisable()
        {
            ResultsIO.endResultsSave -= ConfirmResultsSaved;
        }

        /// ------------------------------------------------------------------------------------
        /// Initial setup methods

        public virtual void SetName()
        {
            testTypeName = "Untitled Logic";
        }

        public void SetSuiteName(string name)
        {
            testSuiteName = name;
        }

        public abstract void SetModel(TestModel inputModel);

        public abstract void SetResultsType();

        public abstract void SetDisplayType();

        /// ------------------------------------------------------------------------------------
        /// Main logic flow methods

        // Set initial information for test at beginning of test run
        public void SetupTest(TestList.TestInfo inputInfo, int inputIndex, string suiteName, RunnerType run)
        {
            ProgressScreen.Instance.SetState(true, ProgressType.LocalSave, "Preparing test");
            SetSuiteName(suiteName);
            SetResultsType();
            activeTestInfo = inputInfo;
            activeRunType = run;
            SetupResultsStructs();
            CheckForBaseline();
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log(this.GetType().Name + " set up test " + activeTestInfo.TestName);
            TestPreProcess();
        }

        // First injection point for custom code. Runs before any test logic.
        public virtual void TestPreProcess()
        {
            // Custom test pre-processing logic here
            StartTest();
        }

        // Logic for test start. Determines code path for test logic.
        public void StartTest()
        {
            ProgressScreen.Instance.SetState(true, ProgressType.LocalSave, "Running test");
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log(this.GetType().Name + " starting test " + activeTestInfo.TestName);
            /*if (stateType == StateType.CreateBaseline)
            {
                if (Master.Instance.debugMode == Master.DebugMode.Messages)
                    Debug.Log(this.GetType().Name + " is processing references for test " + activeTestInfo.TestName);
                StartCoroutine(ProcessBaseline());
            }
            else if (stateType == StateType.CreateResults)
            {*/
                if (Master.Instance.debugMode == Master.DebugMode.Messages)
                    Debug.Log(this.GetType().Name + " is processing results for test " + activeTestInfo.TestName);
                StartCoroutine(ProcessResult());
            /*}*/
        }

        // Logic for creating baseline data
        /*public virtual IEnumerator ProcessBaseline()
        {
            yield return null;
            // Custom test baseline processing logic here
            BuildResultsStruct(null); // Null in base logic. Will not run.
        }*/

        // Logic for creating results data
        public virtual IEnumerator ProcessResult()
        {
            yield return null;
            // Custom test result processing logic here
            BuildResultsStruct(null); // Null in base logic. Will not run.
        }

        // Build results after main test logic is completed
        public void BuildResultsStruct(object input)
        {
            if(input != null)
            {
                if (Master.Instance.debugMode == Master.DebugMode.Messages)
                    Debug.Log(this.GetType().Name + " building results for test " + activeTestInfo.TestName);
                activeResultData = input;
                results.Add(activeResultData);
            }
            TestPostProcess();
        }

        // Last injection point for custom code. Runs after all test logic.
        public virtual void TestPostProcess()
        {
            // Custom test post-processing logic here
            EndTest();
        }

        //Logic for test end. Call to end test logic.
        public void EndTest()
        {
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log(this.GetType().Name + " ending test " + activeTestInfo.TestName);
            if (activeRunType == RunnerType.Run)
                SubmitResults((int)stateType);
            else
            {
                GetComponent<TestDisplayBase>().EnableTestViewer(activeResultData);
            }
        }

        // Submit results data to ResultsIO
        public void SubmitResults(int baseline)
        {
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log(this.GetType().Name + " submitting results for test " + activeTestInfo.TestName);
            SendDataToResultsIO(baseline);
        }

        // Called from ConfirmResultsSave delegate when ResultsIO is done saving files
        void ConfirmResultsSaved()
        {
            if (activeRunType == RunnerType.Run)
            {
                ProgressScreen.Instance.SetState(false, ProgressType.LocalSave, "");
                ClearResults();
                BroadcastEndTestAction();
            }
        }

        /// ------------------------------------------------------------------------------------
        /// Utility methods

        // Check for a baseline (called once for every test that is run)
        public void CheckForBaseline()
        {
            ProgressScreen.Instance.SetState(true, ProgressType.LocalLoad, "Retrieving baseline data");
            bool baseline = ResultsIO.Instance.BaselineExists(testSuiteName, "Standard Legacy", testTypeName, activeTestInfo.SceneName, activeTestInfo.TestName);
            stateType = (StateType)(baseline ? 0 : 1);
        }

        // Get the results struct as an object (never called from the base class as there is no Type for the object)
        public object GetResultsStruct()
        {
            return activeResultData;
        }

        // Clear the results (current called after every test)
        public void ClearResults()
        {
            results.Clear();
        }

        /// ------------------------------------------------------------------------------------
        /// TestViewer related methods
        /// TODO - Revisit this when rewriting the TestViewer

        // Enable test viewer (if in View mode)
        // TODO - Revisit this when rewriting the TestViewer
        /*public virtual void EnableTestViewer()
        {
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log(this.GetType().Name + " enabling Test Viewer");
            ProgressScreen.Instance.SetState(false, ProgressType.LocalSave, "");
            TestViewer.Instance.SetTestViewerState(1, ViewerType.Default, null);
        }*/

        // Called by the TestViewer when ending viewing the current test
        // TODO - Revisit this when rewriting the TestViewer
        public void Cleanup()
        {
            ClearResults();
        }

        // Called by the TestViewer when restarting the current test
        // TODO - Revisit this when rewriting the TestViewer
        public void RestartTest()
        {
            Cleanup();
            StartTest();
        }

        /// ------------------------------------------------------------------------------------
        /// File Data related methods
        /// TODO - Attempt to move even more stuff from final test logic scripts into this section

        // Abstract - Setting up results structs is dependant on each logic's results class
        public abstract void SetupResultsStructs();

        // Send results data to ResultsIO
        public void SendDataToResultsIO(int baseline)
        {
            ProgressScreen.Instance.SetState(true, ProgressType.LocalSave, "Submitting results");
            ResultsIOData output = SerializeResults();
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log(this.GetType().Name + " is sending data to ResultsIO");
            ResultsIO.Instance.ProcessResults(testSuiteName, testTypeName, output, baseline);
        }

        // Serialize ResultsData(class) to ResultsIOData(string arrays)
        public ResultsIOData SerializeResults()
        {
            ResultsIOData output = new ResultsIOData();
            for (int r = 0; r < results.Count + 1; r++)
                output.resultsRow.Add(new ResultsIORow());
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            FieldInfo[] commonFields = typeof(ResultsDataCommon).GetFields(bindingFlags);
            FieldInfo[] customFields = resultsType.GetFields(bindingFlags);
            //output.resultsRow[0] = new ResultsIORow();
            for (int f = 0; f < commonFields.Length; f++)
                output.resultsRow[0].resultsColumn.Add(commonFields[f].Name);
            for (int f = 1; f < customFields.Length; f++)
                output.resultsRow[0].resultsColumn.Add(customFields[f].Name);
            for (int r = 0; r < results.Count; r++)
            {
                FieldInfo commonField = results[r].GetType().GetField("common");
                var commonFieldValue = commonField.GetValue(results[0]);
                output.resultsRow[r + 1].commonResultsIOData = (ResultsDataCommon)commonFieldValue;
                output.resultsRow[r + 1].resultsColumn = new List<string>();

                for (int f = 0; f < commonFields.Length; f++)
                {
                    var typedResult = Convert.ChangeType(results[r], resultsType); // TODO - Why does this work...
                    FieldInfo typedCommonField = typedResult.GetType().GetField("common"); // TODO - Why does this work...
                    var typedCommonValue = Convert.ChangeType(typedCommonField.GetValue(typedResult), resultsCommonTemplate.GetType()); // TODO - Why does this work...
                    var commonResult = typedCommonValue.GetType().GetField(commonFields[f].Name).GetValue(typedCommonValue);
                    output.resultsRow[r + 1].resultsColumn.Add(commonResult.ToString());
                }
                for (int f = 1; f < customFields.Length; f++)
                {
                    var customResult = results[0].GetType().GetField(customFields[f].Name).GetValue(results[r]);
                    if (results[0].GetType().GetField(customFields[f].Name).FieldType.IsArray) //If its an array (tough to handle)
                    {
                        Array a = (Array)results[0].GetType().GetField(customFields[f].Name).GetValue(results[r]);
                        if (a != null) // Null check incase custom results werent set on an array
                        {
                            string[] stringArray = new string[a.Length];
                            for (int i = 0; i < a.Length; i++)
                                stringArray[i] = a.GetValue(i).ToString();
                            customResult = Common.ConvertStringArrayToString(stringArray);
                            output.resultsRow[r + 1].resultsColumn.Add(customResult.ToString());
                        }
                        else // Write blank when custom results werent set on an array
                            customResult = "";
                    }
                    else if (customResult != null) //If its a non-array type that has had values set
                        output.resultsRow[r + 1].resultsColumn.Add(customResult.ToString());
                    else //If its a non-array type that has not had values set
                        output.resultsRow[r + 1].resultsColumn.Add("");
                }
            }
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log(this.GetType().Name + " generated resultsIO data");
            return output;
        }

        // Deserialize ResultsIOData(string arrays) to ResultsData(class)
        public object DeserializeResults(ResultsIOData resultsIOData)
        {
            var resultData = Convert.ChangeType(activeResultData, resultsType); //blank results data
            var common = new ResultsDataCommon(); //blank common data

            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            FieldInfo[] commonFields = typeof(ResultsDataCommon).GetFields(bindingFlags);
            FieldInfo[] customFields = resultsType.GetFields(bindingFlags);

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
                        FieldInfo fieldInfo = common.GetType().GetField(commonFields[cf].Name);
                        fieldInfo.SetValue(common, Convert.ChangeType(value, fieldInfo.FieldType));
                    }
                }
                else
                {
                    var value = resultsDataRaw[(f * 2) - 1];
                    FieldInfo fieldInfo = resultData.GetType().GetField(customFields[f].Name);
                    if (fieldInfo.FieldType.IsArray) // This handles arrays
                    {
                        Type type = resultData.GetType().GetField(customFields[f].Name).FieldType.GetElementType();
                        GenerateGenericArray(fieldInfo, resultData.GetType(), resultData, type, value);
                    }
                    else // Non array types
                    {
                        fieldInfo.SetValue(resultData, Convert.ChangeType(value, fieldInfo.FieldType));
                    }
                }
            }
            return resultData;
        }

        //Convert an array on unknown type to a typed array
        // TODO - Revisit this. Requires hard coding a conversion for each object type
        public void GenerateGenericArray(FieldInfo fieldInfo, Type resultType, object resultObject, Type arrayType, string arrayValue)
        {
            var resultData = System.Convert.ChangeType(resultObject, resultType);
            string[] stringArray = arrayValue.Split(new string[1] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log(this.GetType().Name + " is generating Generic Array for " + resultData + " of type " + arrayType.ToString());
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
                case "System.Byte":
                    byte[] byteArray = new byte[stringArray.Length];
                    for (int i = 0; i < byteArray.Length-1; i++)
                        byteArray[i] = Convert.ToByte (stringArray[i]);
                    fieldInfo.SetValue(resultData, byteArray);
                    break;
            }
        }
    }

    public abstract class TestLogic<T> : TestLogicBase where T : TestModel
    {
        public T model { get; set; }

        public abstract override void SetModel(TestModel inputModel);
    }
}
