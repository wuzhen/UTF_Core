using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // TestLogicBase
    // - Lowest level TestLogic class that all logics derive from
    // - Hides most logic away from end user

    public abstract class TestLogicBase : MonoBehaviour
    {
        // ------------------------------------------------------------------------------------
        // Variables

        // Basic
        [HideInInspector] public TestEntry activeTestEntry;
        [HideInInspector] public bool baselineExists;
        public RunnerType activeRunType;

        // Type Specific
        [HideInInspector] public string testTypeName; // TODO - Find a way to remove this
        public Type display { get; set; } // Reference to the logics display type

        // Results
        public object baseline; //Baseline to compare to (cast to logic's result class)
        public object activeResultData; //Results data to write to (cast to logic's result class)
        public Type resultsType; // Type specific results class to cast to

        // ------------------------------------------------------------------------------------
        // Broadcast

        // Broadcast to TestList when test has ended
        public static event Broadcast.EndTestAction endTestAction;
		public void BroadcastEndTestAction ()
		{
			if (endTestAction != null)
				endTestAction ();
		}

        // Subscribe to event delegates
        void OnEnable()
        {
            ResultsIO.endResultsSave += ConfirmResultsSaved;
        }

        // Desubscribe from event delegates
        void OnDisable()
        {
            ResultsIO.endResultsSave -= ConfirmResultsSaved;
        }

        // ------------------------------------------------------------------------------------
        // Initialization

        public void SetName()
        {
            testTypeName = this.GetType().ToString().Replace("GraphicsTestFramework.", "").Replace("Logic", "");
        }

        public abstract void SetModel(TestModel inputModel);

        public abstract void SetDisplay();

        public abstract void SetResults();

        //public abstract void SetDisplay();

        // ------------------------------------------------------------------------------------
        // Test Execution

        // Set initial information for test at beginning of test run
        public void SetupTest(TestEntry inputEntry, RunnerType runType)
        {
            ProgressScreen.Instance.SetState(true, ProgressType.LocalSave, "Preparing test"); // Enable ProgressScreen
            activeTestEntry = inputEntry; // Store active TestEntry
            activeRunType = runType; // Store active RunnerType
            SetupResultsStructs(); // Setup the results structs to be filled
            CheckForBaseline(); // Check for baselines
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, this.GetType().Name + " set up test " + activeTestEntry.testName); // Write to console
            TestPreProcess(); // Start pre-process
        }

        // First injection point for custom code. Runs before any test logic.
        public virtual void TestPreProcess()
        {
            // Custom test pre-processing logic here
            StartTest(); // Start test
        }

        // Start main test logic
        public void StartTest()
        {
            ProgressScreen.Instance.SetState(true, ProgressType.LocalSave, "Running test"); // Enable ProgressScreen
            Console.Instance.Write(DebugLevel.Logic, MessageLevel.Log, this.GetType().Name + " started test " + activeTestEntry.testName); // Write to console
            StartCoroutine(ProcessResult()); // Process test results
        }

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
            if(input != null) // Null check
            {
                Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, this.GetType().Name + " building results for test " + activeTestEntry.testName); // Write to console
                activeResultData = input; // Set active results data
            }
            TestPostProcess(); // Start post-process
        }

        // Last injection point for custom code. Runs after all test logic.
        public virtual void TestPostProcess()
        {
            // Custom test post-processing logic here
            EndTest(); // End test
        }

        //Logic for test end. Call to end test logic.
        public void EndTest()
        {
            Console.Instance.Write(DebugLevel.Logic, MessageLevel.Log, this.GetType().Name + " ending test " + activeTestEntry.testName); // Write to console
            if (activeRunType == RunnerType.Automation) // If automation run
                SubmitResults(baselineExists ? 0 : 1); // Submit results
            else // If manual run
                GetComponent<TestDisplayBase>().EnableTestViewer(activeResultData); // Enable test viewer with active results data
        }

        // ------------------------------------------------------------------------------------
        // Results Methods

        // Abstract - Setting up results structs is dependant on each logic's results class
        public abstract void SetupResultsStructs();

        // Get the results struct as an object (never called from the base class as there is no Type for the object)
        public object GetResultsStruct()
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, this.GetType().Name + " getting results struct"); // Write to console
            return activeResultData; // Return the active results
        }

        // Submit results data to ResultsIO
        public void SubmitResults(int baseline)
        {
            ProgressScreen.Instance.SetState(true, ProgressType.LocalSave, "Submitting results"); // Enable ProgressScreen
            ResultsIOData output = SerializeResults(); // Serialize activeResultsData
            Console.Instance.Write(DebugLevel.Logic, MessageLevel.Log, this.GetType().Name + " sending data to ResultsIO for " + activeTestEntry.testName); // Write to console
            ResultsIO.Instance.ProcessResults(activeTestEntry.suiteName, activeTestEntry.typeName/*testTypeName*/, output, baseline); // Send data to ResultsIO
        }

        // Called from ConfirmResultsSave delegate when ResultsIO is done saving files
        void ConfirmResultsSaved()
        {
            if (activeRunType == RunnerType.Automation) // Only run if automation
            {
                Console.Instance.Write(DebugLevel.Logic, MessageLevel.Log, this.GetType().Name + " confirmed results save for test " + activeTestEntry.testName); // Write to console
                ProgressScreen.Instance.SetState(false, ProgressType.LocalSave, ""); // Disable ProgressScreen
                BroadcastEndTestAction(); // Broadcast to TestList that rest is completed
            }
        }

        // ------------------------------------------------------------------------------------
        // Display Methods

        // Called by the TestViewer when restarting the current test
        // TODO - Revisit this when rewriting the TestViewer
        public void RestartTest()
        {
            StartTest(); // Restart
        }

        // ------------------------------------------------------------------------------------
        // Helper methods
        // TODO - Clean and comment

        // Check for a baseline (called once for every test that is run)
        public void CheckForBaseline()
        {
            ProgressScreen.Instance.SetState(true, ProgressType.LocalLoad, "Retrieving baseline data"); // Enable ProgressScreen
            baselineExists = ResultsIO.Instance.BaselineExists(activeTestEntry.suiteName, "Standard Legacy", activeTestEntry.typeName/*testTypeName*/, activeTestEntry.sceneName, activeTestEntry.testName); // TODO - Move to TestEntry
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
                    for (int i = 0; i < byteArray.Length - 1; i++)
                        byteArray[i] = Convert.ToByte(stringArray[i]);
                    fieldInfo.SetValue(resultData, byteArray);
                    break;
            }
        }

        // ------------------------------------------------------------------------------------
        // Serialization Methods
        // TODO - Clean and comment (DANGER)

        // Serialize ResultsData(class) to ResultsIOData(string arrays)
        public ResultsIOData SerializeResults()
        {
            ResultsIOData output = new ResultsIOData();
            for (int r = 0; r < 2; r++)
                output.resultsRow.Add(new ResultsIORow());
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            FieldInfo[] commonFields = typeof(ResultsDataCommon).GetFields(bindingFlags);
            FieldInfo[] customFields = resultsType.GetFields(bindingFlags);

            for (int f = 0; f < commonFields.Length; f++)
                output.resultsRow[0].resultsColumn.Add(commonFields[f].Name);
            for (int f = 1; f < customFields.Length; f++)
                output.resultsRow[0].resultsColumn.Add(customFields[f].Name);
            
            FieldInfo commonField = activeResultData.GetType().GetField("common");
            var commonFieldValue = commonField.GetValue(activeResultData);
            output.resultsRow[1].commonResultsIOData = (ResultsDataCommon)commonFieldValue;
            output.resultsRow[1].resultsColumn = new List<string>();
            ResultsDataCommon resultsCommonTemplate = new ResultsDataCommon();
            for (int f = 0; f < commonFields.Length; f++)
            {
                var typedResult = Convert.ChangeType(activeResultData, resultsType); // TODO - Why does this work...
                FieldInfo typedCommonField = typedResult.GetType().GetField("common"); // TODO - Why does this work...
                var typedCommonValue = Convert.ChangeType(typedCommonField.GetValue(typedResult), resultsCommonTemplate.GetType()); // TODO - Why does this work...
                var commonResult = typedCommonValue.GetType().GetField(commonFields[f].Name).GetValue(typedCommonValue);
                output.resultsRow[1].resultsColumn.Add(commonResult.ToString());
            }
            for (int f = 1; f < customFields.Length; f++)
            {
                var customResult = activeResultData.GetType().GetField(customFields[f].Name).GetValue(activeResultData);
                if (activeResultData.GetType().GetField(customFields[f].Name).FieldType.IsArray) //If its an array (tough to handle)
                {
                    Array a = (Array)activeResultData.GetType().GetField(customFields[f].Name).GetValue(activeResultData);
                    if (a != null) // Null check incase custom results werent set on an array
                    {
                        string[] stringArray = new string[a.Length];
                        for (int i = 0; i < a.Length; i++)
                            stringArray[i] = a.GetValue(i).ToString();
                        customResult = Common.ConvertStringArrayToString(stringArray);
                        output.resultsRow[1].resultsColumn.Add(customResult.ToString());
                    }
                    else // Write blank when custom results werent set on an array
                        customResult = "";
                }
                else if (customResult != null) //If its a non-array type that has had values set
                    output.resultsRow[1].resultsColumn.Add(customResult.ToString());
                else //If its a non-array type that has not had values set
                    output.resultsRow[1].resultsColumn.Add("");
            }
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log(this.GetType().Name + " generated resultsIO data");
            return output;
        }

        // Deserialize ResultsIOData(string arrays) to ResultsData(class)
        public object DeserializeResults(ResultsIOData resultsIOData)
        {
            Debug.LogWarning(resultsType);
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
    }

    // ------------------------------------------------------------------------------------
    // TestLogic
    // - Next level TestLogic class that all user facing logics derive from
    // - Adds an abstraction layer for defining model type

    public abstract class TestLogic<M, D> : TestLogicBase where M : TestModel where D : TestDisplayBase
    {
        // ------------------------------------------------------------------------------------
        // Variables

        public M model { get; set; } // Reference to the logics model type        

        // ------------------------------------------------------------------------------------
        // Set Methods

        // Set test model instance
        public override void SetModel(TestModel inputModel)
        {
            model = (M)inputModel; // Cast to type and set
        }

        // Set test display type
        public override void SetDisplay()
        {
            display = typeof(D); // Set type
        }
    }
}
