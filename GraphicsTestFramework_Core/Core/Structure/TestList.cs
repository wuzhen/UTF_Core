using System;
using System.Collections.Generic;
using UnityEngine;

namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // TestList
    // - User defined test list per scene
    // - Contains tests of multiple types
    // - Handles running of tests from the list

    public class TestList : MonoBehaviour
    {
        // ------------------------------------------------------------------------------------
        // Variables

        // Singleton
        private static TestList _Instance = null;
        public static TestList Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = (TestList)FindObjectOfType(typeof(TestList));
                return _Instance;
            }
        }

        // Data
        public List<TestType> testTypes = new List<TestType>();
        Test activeTest;

        // ------------------------------------------------------------------------------------
        // Broadcast

        //Subscribe to event delegates
        void OnEnable()
        {
            TestLogicBase.endTestAction += EndTest;
        }

        //Desubscribe from event delegates
        void OnDisable()
        {
            TestLogicBase.endTestAction -= EndTest;
        }

        // ------------------------------------------------------------------------------------
        // Test Execution

        // Start an individual test (called by TestRunner)
        public void StartTest(TestEntry inputTest, RunnerType runnerType)
        {
            Console.Instance.Write(DebugLevel.Logic, MessageLevel.Log, "Starting test " + inputTest.testName); // Write to console
            activeTest = testTypes[GetTypeIndexInArray(inputTest.typeValue)].tests[inputTest.testIndex]; // Get the active test
            activeTest.testObject.SetActive(true); // Enable the active test object
            TestLogicBase activeTestLogic = GetLogicInstance(activeTest, inputTest); // Get active test logic instance
            activeTestLogic.SetupTest(inputTest, runnerType); // Setup test
        }

        // Get a logic instance and set model instance on it
        TestLogicBase GetLogicInstance(Test activeTest, TestEntry activeEntry)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Getting logic instance"); // Write to console
            TestLogicBase output; // Create logic instance
            TestModelBase activeModelInstance = (TestModelBase)activeTest.testObject.GetComponent(TestTypes.GetTypeFromIndex(testTypes[activeEntry.typeIndex].testType)); // Get the active test model
            activeModelInstance.SetLogic(); // Set the logic reference on the model
            output = TestTypeManager.Instance.GetLogicInstanceFromName(activeModelInstance.logic.ToString().Replace("GraphicsTestFramework.", "").Replace("Logic", "")); // Get test  logic instance
            output.SetModel(activeModelInstance); // Set the active test model in the logic
            TestTypeManager.Instance.SetActiveLogic(output); // Set as active test logic
            return output; // Return
        }

        // End the current Test (called by TestLogic.EndTestAction)
        public void EndTest()
        {
            Console.Instance.Write(DebugLevel.Logic, MessageLevel.Log, "Ended test " + activeTest.testInformation.TestName); // Write to console
            activeTest.testObject.SetActive(false); // Disable the active test object
            ProgressScreen.Instance.SetState(false, ProgressType.LocalLoad, ""); // Disable ProgressScreen
            TestRunner.Instance.FinalizeTest(); // Finalize test on TestRunner
        }

        // ------------------------------------------------------------------------------------
        // Helper Methods

        // Get the array index for a given type by that types index in enum
        int GetTypeIndexInArray(int typeIndex)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Getting type index from type list"); // Write to console
            int output = 0; // Create return integer
            for (int i = 0; i < testTypes.Count; i++) // Iterate test types
            {
                if (testTypes[i].testType == typeIndex) // If type index matches requested
                    output = i; // Get the index for that test type in the test list
            }
            return output; // Return
        }

        // ------------------------------------------------------------------------------------
        // Editor Methods

#if UNITY_EDITOR

        //Every GUI edit
        [ExecuteInEditMode]
        void OnValidate()
        {
            GetModels(); // Get model instances for test objects
            UpdateTestInfo(); // Update info
        }

        //Get models of all test types and add components to test objects
        void GetModels()
        {
            for (int t = 0; t < testTypes.Count; t++) // Iterate test types
            {
                int modelIndex = testTypes[t].testType; // Get index of test type
                for (int r = 0; r < testTypes[t].tests.Count; r++) // Iterate tests of that model
                {
                    if (testTypes[t].tests[r].testObject != null) // If test object exists
                    {
                        Type model = TestTypes.GetTypeFromIndex(modelIndex); // Get model at the index
                        if (!testTypes[t].tests[r].testObject.GetComponent(model)) // If component doesnt already exist
                            testTypes[t].tests[r].testObject.AddComponent(model); // Add it
                    }
                }
            }
        }

        //Update the test info class silently
        void UpdateTestInfo()
        {
            for (int t = 0; t < testTypes.Count; t++) // Iterate test types
            {
                for (int r = 0; r < testTypes[t].tests.Count; r++) // Iterate tests
                {
                    if(testTypes[t].tests[r].testObject) // IF object is assigned
                        testTypes[t].tests[r].testInformation = new TestInfo(testTypes[t].tests[r].testObject.name, gameObject.scene.name); // Set test information
                }
            }
        }
#endif

        // ------------------------------------------------------------------------------------
        // Local Data Structures

        [Serializable]
        public class TestType
        {
            public int testType;
            public List<Test> tests = new List<Test>();
        }

        [Serializable]
        public class Test
        {
            [HideInInspector] public TestInfo testInformation;
            public GameObject testObject;
        }

        [Serializable]
        public class TestInfo
        {
            public string TestName;
            public string SceneName;

            public TestInfo(string inputName, string inputScene)
            {
                TestName = inputName;
                SceneName = inputScene;
            }
        }
    }
}
