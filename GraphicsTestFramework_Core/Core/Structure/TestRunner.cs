using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // Global Enums

    public enum RunnerType
    {
        Automation,
        Manual,
        Resolve
    };

    // ------------------------------------------------------------------------------------
    // TestRunner
    // - Global test execution management
    // - Converts TestStructure to a runner based on menu selections
    // - Handles running of tests across multiple TestList

    public class TestRunner : MonoBehaviour
    {
        // ------------------------------------------------------------------------------------
        // Variables

        // Singleton
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

        // Data
        public RunnerType runnerType;
        public Runner runner;
        bool runnerIsWaiting;
        int currentTestIndex;

        // Level load (TODO - Update API)
        private bool levelWasLoaded = false;
        private void OnLevelWasLoaded(int iLevel)
        {
            levelWasLoaded = true;
        }

        // ------------------------------------------------------------------------------------
        // Initialization

        // Setup the runner after instantiation
        public void SetupRunner(RunnerType runType)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Setting up runner"); // Write to console
            ProgressScreen.Instance.SetState(true, ProgressType.LocalLoad, "Generating and processing new Test Runner"); // Enable ProgressScreen
            runnerType = runType; // Set runner type
            GenerateTestRunner(TestStructure.Instance.GetStructure()); // Generate test runner
        }

        // Convert the test structure into a runner based on current selection and runner type
        void GenerateTestRunner(TestStructure.Structure inputStructure)
        {
            runner = new Runner(); // Create new Runner instance
            for (int su = 0; su < inputStructure.suites.Count; su++) // Iterate suites
            {
                if (inputStructure.suites[su].selectionState != 0 || runnerType != RunnerType.Automation) // If selected or automation
                {
                    string suiteName = inputStructure.suites[su].suiteName;
                    for (int ty = 0; ty < inputStructure.suites[su].types.Count; ty++) // Iterate types
                    {
                        if (inputStructure.suites[su].types[ty].selectionState != 0 || runnerType != RunnerType.Automation) // If selected or automation
                        {
                            string typeName = inputStructure.suites[su].types[ty].typeName;
                            int typeIndex = inputStructure.suites[su].types[ty].typeIndex;
                            for (int sc = 0; sc < inputStructure.suites[su].types[ty].scenes.Count; sc++) // Iterate scenes
                            {
                                if (inputStructure.suites[su].types[ty].scenes[sc].selectionState != 0 || runnerType != RunnerType.Automation) // If selected or automation
                                {
                                    string sceneName = inputStructure.suites[su].types[ty].scenes[sc].sceneName;
                                    string scenePath = inputStructure.suites[su].types[ty].scenes[sc].scenePath;
                                    for (int te = 0; te < inputStructure.suites[su].types[ty].scenes[sc].tests.Count; te++) // Iterate tests
                                    {
                                        if (inputStructure.suites[su].types[ty].scenes[sc].tests[te].selectionState != 0 || runnerType != RunnerType.Automation) // If selected or automation
                                        {
                                            string testName = inputStructure.suites[su].types[ty].scenes[sc].tests[te].testName;
                                            if (!inputStructure.suites[su].types[ty].scenes[sc].tests[te].baseline || runnerType != RunnerType.Resolve) // If baseline resolution mode return all with no baselines
                                            {
                                                TestEntry newTest = new TestEntry(suiteName, sceneName, scenePath, typeName, testName, typeIndex, su, sc, ty, te); // Create new TestEntry instance
                                                runner.tests.Add(newTest); // Add to runner
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            Console.Instance.Write(DebugLevel.Logic, MessageLevel.Log, "Generated test runner"); // Write to console
            if (runnerType != RunnerType.Resolve) // If in Resolve mode we start tests manually
                StartTests(); // Start tests
        }

        // ------------------------------------------------------------------------------------
        // Runner Execution

        public void StartTests()
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Starting runner execution"); // Write to console
            Menu.Instance.SetMenuState(0); // Disable menu
            switch (runnerType) // Switch execution logic based on RunnerType
            {
                case RunnerType.Automation: // Automation ("Run" in menu)
                    StartCoroutine(IterateTests()); // Iterate the runner
                    break;
                case RunnerType.Manual: // Manual ("View" in menu)
                    LoadSelectedTest(); // Load selected from menu
                    break;
                case RunnerType.Resolve: // Resolve (When forcing baseline resolution in menu)
                    currentTestIndex = 0; // Current index is always 0
                    StartCoroutine(LoadTest()); // Load tests manually from 0
                    break;
            }
        }

        // Iterate tests (Automation)
        IEnumerator IterateTests()
        {
            Console.Instance.Write(DebugLevel.Logic, MessageLevel.Log, "Starting automation run"); // Write to console
            for (int i = 0; i < runner.tests.Count; i++) // Iterate tests
            {
                do { yield return null; } while (runnerIsWaiting == true); // Wait for previous test to finish before next test
                runnerIsWaiting = true; // Set waiting
                currentTestIndex = i; // Set current test index
                StartCoroutine(LoadTest()); // Load test
            }
            do { yield return null; } while (runnerIsWaiting == true); // Wait for previous test to finish before enabling menus
            Console.Instance.Write(DebugLevel.Logic, MessageLevel.Log, "Ended automation run"); // Write to console
            Menu.Instance.SetMenuState(1); // Enable menu
        }

        // Load Test of currentTestIndex
        IEnumerator LoadTest()
        {
            if (SceneManager.GetActiveScene().name != runner.tests[currentTestIndex].sceneName) // If current scene name does not match requested
            {
                SceneManager.LoadScene(runner.tests[currentTestIndex].sceneName); // Load requested scene
                while (!levelWasLoaded) // Wait for load
                    yield return null;
                levelWasLoaded = false; // Reset
            }
            Console.Instance.Write(DebugLevel.Logic, MessageLevel.Log, "Loading test "+ runner.tests[currentTestIndex].testName); // Write to console
            TestList.Instance.StartTest(runner.tests[currentTestIndex], runnerType); // Start the test
        }

        // Load the currently selected test for view
        void LoadSelectedTest()
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Loading selected test"); // Write to console
            MenuTestEntry selectedEntry = Menu.Instance.GetSelectedEntry(); // Get selected menu entry
            currentTestIndex = FindSelectedTestIndex(selectedEntry); // Find selected entry in runner (will be -1 in failure case)
            StartCoroutine(LoadTest()); // Start load
        }

        // Find and load previous test
        public void PreviousTest()
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Selecting previous test"); // Write to console
            if (currentTestIndex > 0) // If not first test
                currentTestIndex--; // Select previous test
            else
                currentTestIndex = runner.tests.Count - 1; // Select last test
            StartCoroutine(LoadTest()); // Load test
        }

        // Find and load next test
        public void NextTest()
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Selecting next test"); // Write to console
            if (currentTestIndex < runner.tests.Count - 1) // If not last test
                currentTestIndex++; // Select next test
            else
                currentTestIndex = 0; // Select first test
            StartCoroutine(LoadTest()); // Load test
        }

        // Finalise test (Called by TestList.EndTest)
        public void FinalizeTest()
        {
            Console.Instance.Write(DebugLevel.Logic, MessageLevel.Log, "Finalizing test " + runner.tests[currentTestIndex].testName); // Write to console
            runnerIsWaiting = false; // To waiting to false so automation can continue
        }

        // ------------------------------------------------------------------------------------
        // Get Data

        // Get the entry for the current test index
        public TestEntry GetCurrentTestEntry()
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Getting current test entry"); // Write to console
            return runner.tests[currentTestIndex]; // Return current test entry
        }

        // ------------------------------------------------------------------------------------
        // Helper Methods

        // Find the runner index of the selected test
        int FindSelectedTestIndex(MenuTestEntry selectedEntry)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Finding selected test index"); // Write to console
            for (int i = 0; i < runner.tests.Count; i++) // Iterate tests
            {
                if (runner.tests[i].suiteIndex == selectedEntry.suiteId &&
                    runner.tests[i].sceneIndex == selectedEntry.sceneId &&
                    runner.tests[i].typeIndex == selectedEntry.typeId &&
                    runner.tests[i].testIndex == selectedEntry.testId) // If all data matches
                {
                    return i; // Return it
                }
            }
            Console.Instance.Write(DebugLevel.Critical, MessageLevel.LogError, "Test index not found!"); // Write to console
            return -1; // Return failure
        }

        // Check if the current test is the last in the current runner
        public bool CheckEndOfRunner()
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Checking for end of runner"); // Write to console
            if (currentTestIndex < runner.tests.Count - 1) // If tests remain
                return false; // Return false
            else
                return true; // Return true
        }

        // ------------------------------------------------------------------------------------
        // Local Data Structures

        [Serializable]
        public class Runner
        {
            public List<TestEntry> tests = new List<TestEntry>();
        }
    }

    // ------------------------------------------------------------------------------------
    // Global Data Structures

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

        public TestEntry(string inputSuiteName, string inputSceneName, string inputScenePath, string inputTypeName, string inputTestName, int inputTypeValue, int inputSuiteIndex, int inputSceneIndex, int inputTypeIndex, int inputTestIndex)
        {
            suiteName = inputSuiteName;
            sceneName = inputSceneName;
            scenePath = inputScenePath;
            typeName = inputTypeName;
            testName = inputTestName;
            typeValue = inputTypeValue;
            suiteIndex = inputSuiteIndex;
            sceneIndex = inputSceneIndex;
            typeIndex = inputTypeIndex;
            testIndex = inputTestIndex;
        }
    }
}
