using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GraphicsTestFramework
{
    public enum RunnerType { Default, Run, View, ResolveBaseline };
    
    public class TestRunner : MonoBehaviour
    {
        public RunnerType runnerType;
        TestStructure.Structure testStructure;
        public TestID selectedEntry;
        public Runner runner;
        bool runnerIsWaiting;
        int currentTestIndex;
        //TestList activeTestList;

        private static TestRunner _Instance = null;
        public static TestRunner Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = (TestRunner)FindObjectOfType(typeof(TestRunner));
                return _Instance;
            }
        }

        private bool levelWasLoaded = false;
        private void OnLevelWasLoaded(int iLevel)
        {
            levelWasLoaded = true;
        }

        /// ------------------------------------------------------------------------------------
        /// Setup

        // Setup the runner after instantiation
        public void SetupRunner(RunnerType runType)
        {
            ProgressScreen.Instance.SetState(true, ProgressType.LocalLoad, "Generating and processing new Test Runner");
            runnerType = runType;
            switch(runnerType)
            {
                case RunnerType.Default: // TODO - This mode should never be passed to the Runner. Remove when sure.
                    Debug.LogWarning("Default runner type was passed to the TestRunner. This shouldnt happen!");
                    break;
                case RunnerType.Run:
                    break;
                case RunnerType.View:
                    GetSelectedEntry();
                    break;
                case RunnerType.ResolveBaseline:
                    break;
            }
            GetTestStructure();
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log("Test Runner finished setup");
            GenerateTestRunner();
        }
        
        // Get the final test structure ready to convert to a runner
        void GetTestStructure()
        {
            testStructure = TestStructure.Instance.GetStructure();
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log("Test Runner received test structure");
        }

        // Get the selected entry from the menu to start the runner at a specific point
        void GetSelectedEntry()
        {
            selectedEntry = Menu.Instance.GetSelectedEntry();
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log("Test Runner receieved selected entry");
        }

        /// ------------------------------------------------------------------------------------
        /// Generate a new runner

        // Convert the test structure into a runner based on current selection and runner type
        void GenerateTestRunner()
        {
            runner = new Runner();
            for (int su = 0; su < testStructure.suites.Count; su++)
            {
                if (testStructure.suites[su].selectionState != 0 || runnerType != RunnerType.Run)
                {
                    for (int ty = 0; ty < testStructure.suites[su].types.Count; ty++)
                    {
                        if (testStructure.suites[su].types[ty].selectionState != 0 || runnerType != RunnerType.Run) // TODO - This check works?
                        {
                            for (int sc = 0; sc < testStructure.suites[su].types[ty].scenes.Count; sc++)
                            {
                                if (testStructure.suites[su].types[ty].scenes[sc].selectionState != 0 || runnerType != RunnerType.Run) // TODO - This check works?
                                {
                                    for (int te = 0; te < testStructure.suites[su].types[ty].scenes[sc].tests.Count; te++)
                                    {
                                        if (testStructure.suites[su].types[ty].scenes[sc].tests[te].selectionState != 0 || runnerType != RunnerType.Run)
                                        {
                                            // This last check is for baseline resolution mode. If in that runner type only returns all tests missing baselines
                                            if(!testStructure.suites[su].types[ty].scenes[sc].tests[te].baseline || runnerType != RunnerType.ResolveBaseline)
                                            {
                                                TestEntry newTest = new TestEntry();
                                                newTest.suiteName = testStructure.suites[su].suiteName;
                                                newTest.sceneName = testStructure.suites[su].types[ty].scenes[sc].sceneName;
                                                newTest.scenePath = testStructure.suites[su].types[ty].scenes[sc].scenePath;
                                                newTest.typeName = testStructure.suites[su].types[ty].typeName;
                                                newTest.testName = testStructure.suites[su].types[ty].scenes[sc].tests[te].testName;
                                                newTest.typeValue = testStructure.suites[su].types[ty].typeIndex;
                                                newTest.suiteIndex = su;
                                                newTest.sceneIndex = sc;
                                                newTest.typeIndex = ty;
                                                newTest.testIndex = te;
                                                runner.tests.Add(newTest);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log("Test Runner generated runner");
            if(runnerType != RunnerType.ResolveBaseline) // If RunnerType == ResolveBAseline we start tests manually
                StartTests();
        }

        /// ------------------------------------------------------------------------------------
        /// Run execution
        
        public void StartTests()
        {
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log("Test Runner started executing the run");
            Menu.Instance.SetMenuState(0);
            switch(runnerType)
            {
                case RunnerType.Default: // TODO - This mode should never be passed to the Runner. Remove when sure.
                    Debug.LogWarning("Default runner type was passed to the TestRunner. This shouldnt happen!");
                    break;
                case RunnerType.Run:
                    StartCoroutine(IterateTests());
                    break;
                case RunnerType.View:
                    LoadSelectedTest();
                    break;
                case RunnerType.ResolveBaseline:
                    currentTestIndex = 0;
                    StartCoroutine(LoadTest());
                    break;
            }
        }

        public void PreviousTest()
        {
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log("Test Runner is selecting previous test");
            if(currentTestIndex > 0)
                currentTestIndex--;
            else
                currentTestIndex = runner.tests.Count - 1;
            StartCoroutine(LoadTest());
        }

        public void NextTest()
        {
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log("Test Runner is selecting next test");
            if(currentTestIndex < runner.tests.Count - 1)
                currentTestIndex++;
            else
                currentTestIndex = 0;
            StartCoroutine(LoadTest());
        }

        IEnumerator LoadTest()
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            string requestedSceneName = runner.tests[currentTestIndex].sceneName;
            if(currentSceneName != requestedSceneName)
            {
                SceneManager.LoadScene(requestedSceneName);
                while (!levelWasLoaded)
                    yield return null;
                levelWasLoaded = false;
                //activeTestList = FindObjectOfType<TestList>();
            }
            //if(activeTestList == null)
               // activeTestList = FindObjectOfType<TestList>();
            if(TestList.Instance == null)
                Debug.LogWarning("Test List not found!");

            //activeTestList.Setup(runner.tests[currentTestIndex].suiteName);
            TestList.Instance.Setup(runner.tests[currentTestIndex].suiteName);
            yield return new WaitForEndOfFrame();
            TestInstance newTest = new TestInstance(runner.tests[currentTestIndex].typeValue, runner.tests[currentTestIndex].testIndex);
            selectedEntry = null; // Null this so it doesnt affect next iteration
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log("Test Runner is loading test");
            //activeTestList.RunTest(newTest, runnerType);
            TestList.Instance.RunTest(newTest, runnerType);
        }

        // Load the currently selected test for view
        void LoadSelectedTest()
        {
            GetSelectedEntry();
            currentTestIndex = FindSelectedTestIndex();
            StartCoroutine(LoadTest());
        }

        // Find the runner index of the selected test
        int FindSelectedTestIndex()
        {
            for(int i = 0; i < runner.tests.Count; i++)
            {
                if (runner.tests[i].suiteIndex == selectedEntry.suiteId &&
                    runner.tests[i].sceneIndex == selectedEntry.sceneId &&
                    runner.tests[i].typeIndex == selectedEntry.typeId &&
                    runner.tests[i].testIndex == selectedEntry.testId)
                {
                    return i;
                }
            }
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.LogWarning("Test index not found!");
            return -1;
        }

        // Iterate tests in Run mode
        IEnumerator IterateTests()
        {
            for(int i = 0; i < runner.tests.Count; i++)
            {
                do { yield return null; } while (runnerIsWaiting == true);
                runnerIsWaiting = true;
                currentTestIndex = i;
                StartCoroutine(LoadTest());
            }
            do { yield return null; } while (runnerIsWaiting == true);
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log("Test Runner finished executing the run");
            Menu.Instance.SetMenuState(1);
        }

        // Finalise test in Run mode so iteration can continue
        public void FinaliseTest()
        {
            runnerIsWaiting = false;
        }

        /// ------------------------------------------------------------------------------------
        /// Get Data from Runner

        // Get the entry for the current test index
        public TestEntry GetCurrentTestEntry()
        {
            return runner.tests[currentTestIndex];
        }

        // Check if the current test is the last in the current runner
        public bool CheckEndOfRunner()
        {
            if (currentTestIndex < runner.tests.Count - 1)
                return false;
            else
                return true;
        }

        /// ------------------------------------------------------------------------------------
        /// Member Data Structures

        [Serializable]
        public class Runner
        {
            //public List<Suite> suites = new List<Suite>();
            public List<TestEntry> tests = new List<TestEntry>();
        }

        [Serializable]
        public class TestEntry
        {
            public string suiteName;
            public string sceneName;
            public string scenePath;
            public string typeName;
            public string testName;
            public int typeValue;
            public int suiteIndex;
            public int sceneIndex;
            public int typeIndex;
            public int testIndex;
        }
    }

    /// ------------------------------------------------------------------------------------
    /// Public Data Structures

    [Serializable]
    public class TestInstance // Sent to test list
    {
        public int typeValue;
        public int testIndex;

        public TestInstance(int typeVal, int testInt)
        {
            typeValue = typeVal;
            testIndex = testInt;
        }
    }
}
