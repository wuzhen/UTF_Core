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

        // Global Data
        public List<TestType> testTypes = new List<TestType>();
        [HideInInspector] public TestLogicBase activeTestLogic;
        [HideInInspector] public TestDisplayBase activeTestDisplay;

        // Local Data
        List<Type> modelList = new List<Type>();
        string suiteName;
        public RunnerType currentRunnerType;
        public int currentTypeIndex;
        public int currentTestIndex;

        /// ------------------------------------------------------------------------------------
        /// Setup

        // Setup the test list (called by TestRunner when scene is loaded)
        public void Setup(string suite)
        {
            suiteName = suite;
            modelList = Common.GetSubTypes<TestModel>();
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log(this.GetType().Name + " finished setup");
        }

        //Subscribe to event delegates
        void OnEnable()
        {
            //ResultsIO.endResultsSave += EndTest;
            TestLogicBase.endTestAction += EndTest;
        }

        //Desubscribe from event delegates
        void OnDisable()
        {
            //ResultsIO.endResultsSave -= EndTest;
            TestLogicBase.endTestAction -= EndTest;
        }

        /// ------------------------------------------------------------------------------------
        /// Test execution

        // Start an individual test (called by TestRunner)
        public void RunTest(TestInstance inputTest, RunnerType runnerType)
        {
            currentRunnerType = runnerType;
            currentTypeIndex = GetTypeIndexInArray(inputTest.typeValue);
            currentTestIndex = inputTest.testIndex;
            Test activeTest = testTypes[currentTypeIndex].tests[currentTestIndex];
            activeTest.testObject.SetActive(true);
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log(this.GetType().Name + " is starting test " + testTypes[currentTypeIndex].tests[currentTestIndex].testName);
            SetupTestInstances(activeTest);
            activeTestLogic.SetupTest(testTypes[currentTypeIndex].tests[currentTestIndex].testInformation, inputTest.typeValue, suiteName, runnerType);
        }

        // TODO - Cleanup
        void SetupTestInstances(Test activeTest)
        {
            TestModel activeModelInstance = (TestModel)activeTest.testObject.GetComponent(modelList[testTypes[currentTypeIndex].testType]);
            activeModelInstance.SetLogic();

            if (Master.Instance.transform.Find("TestRunners"))
            {
                Transform runnerParent = Master.Instance.transform.Find("TestRunners");
                string childName = activeModelInstance.logic.ToString().Replace("GraphicsTestFramework.", "").Replace("Logic", "");
                GameObject activeChild = runnerParent.Find(childName).gameObject;
                activeTestLogic = (TestLogicBase)activeChild.GetComponent(activeModelInstance.logic);
                activeTestDisplay = (TestDisplayBase)activeChild.GetComponent(activeTestLogic.displayType);
            }
            else
                Debug.LogError("Test Runner parent not found! Aborting");

            activeTestLogic.SetModel(activeModelInstance);
        }
        
        // Get the array index for a given type by that types index in enum
        int GetTypeIndexInArray(int typeIndex)
        {
            int output = 0;
            for(int i = 0; i < testTypes.Count; i++)
            {
                if (testTypes[i].testType == typeIndex)
                    output = i;
            }
            return output;
        }

        public void EndTest()
        {
            //int typeIndex = GetTypeIndexInArray(currentTest.typeIndex);
            testTypes[currentTypeIndex].tests[currentTestIndex].testObject.SetActive(false);
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log(this.GetType().Name + " ended test " + testTypes[currentTypeIndex].tests[currentTestIndex].testName);
            ProgressScreen.Instance.SetState(false, ProgressType.LocalLoad, "");
            TestRunner.Instance.FinaliseTest();
        }

        /// ------------------------------------------------------------------------------------
        /// Data helpers

        TestInstance CloneTestInstance(TestInstance input)
        {
            if(input == null)
                Debug.LogWarning("Attempting to clone null test instance");
            TestInstance output = new TestInstance(input.typeValue, input.testIndex);
            return output;
        }

        /// ------------------------------------------------------------------------------------
        /// Editor functions

#if UNITY_EDITOR
        //Every GUI edit
        [ExecuteInEditMode]
        void OnValidate()
        {
            GetModels();
            UpdateTestInfo();
        }

        //Get models of all test types and add components to test objects
        void GetModels()
        {
            List<Type> modelList = Common.GetSubTypes<TestModel>();
            for (int t = 0; t < testTypes.Count; t++)
            {
                int model = testTypes[t].testType;
                for (int r = 0; r < testTypes[t].tests.Count; r++)
                {
                    if (testTypes[t].tests[r].testObject != null)
                    {
                        if (modelList.Count > model)
                        {
                            if (!testTypes[t].tests[r].testObject.GetComponent(modelList[model]))
                                testTypes[t].tests[r].testObject.AddComponent(modelList[model]);
                        }
                    }
                }
            }
        }

        //Update the test info class silently
        void UpdateTestInfo()
        {
            for (int t = 0; t < testTypes.Count; t++)
            {
                for (int r = 0; r < testTypes[t].tests.Count; r++)
                {
                    testTypes[t].tests[r].testName = testTypes[t].tests[r].testObject.name;
                    testTypes[t].tests[r].testInformation = new TestInfo(testTypes[t].tests[r].testName, gameObject.scene.name);
                }
            }
        }
#endif

        /// ------------------------------------------------------------------------------------
        /// Member Data Structures

        [Serializable]
        public class TestType
        {
            public int testType;
            public List<Test> tests = new List<Test>();
        }

        [Serializable]
        public class Test
        {
            public string testName;
            [HideInInspector]
            public TestInfo testInformation;
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
