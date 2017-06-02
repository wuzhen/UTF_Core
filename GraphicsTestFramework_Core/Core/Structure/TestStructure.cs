using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // TestStructure
    // - Gets data from SuiteManager and all TestLists
    // - Generates a structure of all available tests
    // - Handles baseline checks and selection changes on the structure

    public class TestStructure : MonoBehaviour
    {
        // ------------------------------------------------------------------------------------
        // Variables

        // Singleton
        private static TestStructure _Instance = null;
        public static TestStructure Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = (TestStructure)FindObjectOfType(typeof(TestStructure));
                return _Instance;
            }
        }

        // Data
        public Structure testStructure;

        // Level load (TODO - Update API)
        private bool levelWasLoaded = false;
        private void OnLevelWasLoaded(int iLevel)
        {
            levelWasLoaded = true;
        }

        // Generation check
        private bool m_IsGenerated = false;
        public bool IsGenerated
        {
            get { return m_IsGenerated; }
        }

        // ------------------------------------------------------------------------------------
        // Broadcast

        //Subscribe to event delegates
        void OnEnable()
        {
            ResultsIO.baselinesParsed += StartGeneration;
        }

        //Desubscribe from event delegates
        void OnDisable()
        {
            ResultsIO.baselinesParsed -= StartGeneration;
        }

        // ------------------------------------------------------------------------------------
        // Initialization

        // Start generation process
        void StartGeneration()
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Initializing TestStructure"); // Write to console
            ProgressScreen.Instance.SetState(true, ProgressType.LocalLoad, "Generating Test Structure"); // Enable ProgressScreen
            StartCoroutine(GenerateStructure()); // Start generating structure
        }

        // Generate test structure
        // - Loads all scenes and gets data from test lists
        // - Reorganises for menu layout
        IEnumerator GenerateStructure()
        {
            testStructure = new Structure(); // Create new test structure instance
            List<Type> modelList = Common.GetSubTypes<TestModel>(); // Get model list
            for (int su = 0; su < SuiteManager.Instance.suites.Count; su++) // Iterate suites on SuiteManager
            {
                Suite newSuite = new Suite(); // Create new suite instance
                newSuite.suiteName = SuiteManager.Instance.suites[su].suiteName; // Set suite name from SuiteManager
                for (int sc = 0; sc < SuiteManager.Instance.suites[su].scenes.Count; sc++) // Iterate scenes
                {
                    SceneManager.LoadSceneAsync(SuiteManager.Instance.suites[su].scenes[sc].path); // Load scene
                    while (!levelWasLoaded) // Wait for scene load complete
                        yield return null;
                    levelWasLoaded = false; // Reset
                    UnityEngine.SceneManagement.Scene scene = SceneManager.GetSceneAt(0); // Get a scene reference
                    TestList testList = FindObjectOfType<TestList>(); // Get TestList from current scene
                    for (int ty = 0; ty < testList.testTypes.Count; ty++) // Iterate test types
                    {
                        TestModel model = (TestModel)testList.testTypes[ty].tests[0].testObject.GetComponent(modelList[testList.testTypes[ty].testType]); // Get a model reference from the test list
                        TestLogicBase logic = model.GetLogic(); // Get a logic reference from the model
                        model.SetLogic(); // TODO - Need this?
                        logic.SetName(); // Set name on the logic so it exists when checking for baseline
                        TestType newType = FindDuplicateTypeInSuite(newSuite, logic.testTypeName); // Check for duplicate types and return if found
                        if(newType == null) // If no duplicate type was found
                        {
                            newType = new TestType(); // Create a new type instance
                            newType.typeName = logic.testTypeName; // Set type name
                            newType.typeIndex = testList.testTypes[ty].testType;  // Set type index
                            newSuite.types.Add(newType); // Add type to suite
                            TestTypeManager.Instance.GenerateTestTypeInstances(model); // Generate an instance object for test logic/display
                        }
                        Scene newScene = FindDuplicateSceneInType(newSuite, newType, scene.name);  // Check for duplicate scenes and return if found
                        if (newScene == null) // If no duplicate scene was found
                        {
                            newScene = new Scene(); // Create a new scene instance
                            newScene.sceneName = scene.name; // Set scene name
                            newScene.scenePath = scene.path; // Set scene path
                            for (int te = 0; te < testList.testTypes[ty].tests.Count; te++) // Iterate tests
                            {
                                Test newTest = new Test(); // Create new test instance
                                newTest.testName = testList.testTypes[ty].tests[te].testInformation.TestName; // Set test name
                                newScene.tests.Add(newTest); // Add test to scene
                            }
                            newType.scenes.Add(newScene); // Add scene to type
                        }
                    }
                }
                testStructure.suites.Add(newSuite); // Add suite to structure
            }
            m_IsGenerated = true; // Set generated
            Console.Instance.Write(DebugLevel.Logic, MessageLevel.Log, "TestStructure finished generating"); // Write to console
            ProgressScreen.Instance.SetState(false, ProgressType.LocalLoad, ""); // Disable ProgressScreen
        }

        // ------------------------------------------------------------------------------------
        // Helper Methods

        // Check ResultsIO for baselines then write data through the test structure
        public bool CheckForBaselines()
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Checking for baselines"); // Write to console
            bool output = true;
            for(int su = 0; su < testStructure.suites.Count; su++)
            {
                testStructure.suites[su].baseline = true;
                for(int ty = 0; ty < testStructure.suites[su].types.Count; ty++)
                {
                    testStructure.suites[su].types[ty].baseline = true;
                    for(int sc = 0; sc < testStructure.suites[su].types[ty].scenes.Count; sc++)
                    {
                        testStructure.suites[su].types[ty].scenes[sc].baseline = true;
                        for(int te = 0; te < testStructure.suites[su].types[ty].scenes[sc].tests.Count; te++)
                        {
                            bool baseline = ResultsIO.Instance.BaselineExists(testStructure.suites[su].suiteName, "Standard Legacy", testStructure.suites[su].types[ty].typeName, testStructure.suites[su].types[ty].scenes[sc].sceneName, testStructure.suites[su].types[ty].scenes[sc].tests[te].testName);
                            testStructure.suites[su].types[ty].scenes[sc].tests[te].baseline = baseline;
                            if(baseline == false)
                            {
                                testStructure.suites[su].baseline = false;
                                testStructure.suites[su].types[ty].baseline = false;
                                testStructure.suites[su].types[ty].scenes[sc].baseline = false;
                                output = false;
                            }
                        }
                    }
                }
            }
            return output;
        }

        // TODO - Dont use this anymore. Need this?. See GenerateStructure()
        TestType GetTypeIndex(string suiteName, string typeName)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Getting type index for type "+typeName+" in suite "+suiteName); // Write to console
            for (int a = 0; a < testStructure.suites.Count; a++)
            {
                if (testStructure.suites[a].suiteName == suiteName)
                {
                    for (int b = 0; b < testStructure.suites[a].types.Count; b++)
                    {
                        if (testStructure.suites[a].types[b].typeName == typeName)
                        {
                            return testStructure.suites[a].types[b];
                        }
                    }
                }
            }
            return null;
        }

        TestType FindDuplicateTypeInSuite(Suite suite, string name)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Finding duplicates of type "+name+" in suite "+suite.suiteName); // Write to console
            for (int b = 0; b < suite.types.Count; b++)
            {
                if (suite.types[b].typeName == name)
                    return suite.types[b];
            }
            return null;
        }

        Scene FindDuplicateSceneInType(Suite suite, TestType type, string sceneName)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Finding duplicates of scene " + sceneName + " in suite " + suite.suiteName+" and type "+type.typeName); // Write to console
            for (int b = 0; b < suite.types.Count; b++)
            {
                if (suite.types[b].typeName == type.typeName)
                {
                    for (int c = 0; c < type.scenes.Count; c++)
                    {
                        if (type.scenes[c].sceneName == name)
                            return type.scenes[c];
                    }
                }
            }
            return null;
        }

        // Checks whether any tests are selected (true if >0 tests are selected)
        public bool CheckSelectionNotNull()
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Checking for null selection"); // Write to console
            for (int su = 0; su < testStructure.suites.Count; su++)
            {
                if (testStructure.suites[su].selectionState != 0)
                    return true;
            }
            return false;
        }

        // ------------------------------------------------------------------------------------
        // Get Data
        // TODO - Clean and comment this

        // Get the entire structure
        public Structure GetStructure()
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Getting test structure"); // Write to console
            return testStructure; // Return the TestStructure
        }

        // Get a list of entries at a specific level
        public MenuEntryData[] GetEntries(MenuTestEntry input) // TODO - Can we shorten this?
        {
            MenuEntryData[] output = null;
            if (input.currentLevel == 0)
                output = new MenuEntryData[testStructure.suites.Count];
            for (int su = 0; su < testStructure.suites.Count; su++)
            {
                if (input.currentLevel == 0)
                {
                    MenuEntryData entry = new MenuEntryData();
                    MenuTestEntry id = Menu.Instance.CloneMenuID(input);
                    id.suiteId = su;                                // Replace relevent data 
                    entry.entryName = testStructure.suites[su].suiteName;
                    entry.selectionState = testStructure.suites[su].selectionState;
                    entry.id = id;
                    output[su] = entry;
                }
                if (input.currentLevel >= 1)
                {
                    if (su == input.suiteId)
                    {
                        if (input.currentLevel == 1)
                            output = new MenuEntryData[testStructure.suites[su].types.Count];
                        for (int ty = 0; ty < testStructure.suites[su].types.Count; ty++)
                        {
                            if (input.currentLevel == 1 && su == input.suiteId)
                            {
                                MenuEntryData entry = new MenuEntryData();
                                MenuTestEntry id = Menu.Instance.CloneMenuID(input);
                                id.currentLevel = 1;                            // Replace relevent data 
                                id.suiteId = su;                                // Replace relevent data 
                                id.typeId = ty;                                 // Replace relevent data 
                                entry.entryName = testStructure.suites[su].types[ty].typeName;
                                entry.selectionState = testStructure.suites[su].types[ty].selectionState;
                                entry.id = id;
                                output[ty] = entry;
                            }
                            if (input.currentLevel >= 2)
                            {
                                if (ty == input.typeId)
                                {
                                    if (input.currentLevel == 2)
                                        output = new MenuEntryData[testStructure.suites[su].types[ty].scenes.Count];
                                    for (int sc = 0; sc < testStructure.suites[su].types[ty].scenes.Count; sc++)
                                    {
                                        if (input.currentLevel == 2 && ty == input.typeId)
                                        {
                                            MenuEntryData entry = new MenuEntryData();
                                            MenuTestEntry id = Menu.Instance.CloneMenuID(input);
                                            id.currentLevel = 2;                            // Replace relevent data 
                                            id.suiteId = su;                                // Replace relevent data 
                                            id.typeId = ty;                                 // Replace relevent data 
                                            id.sceneId = sc;                                 // Replace relevent data 
                                            entry.entryName = testStructure.suites[su].types[ty].scenes[sc].sceneName;
                                            entry.selectionState = testStructure.suites[su].types[ty].scenes[sc].selectionState;
                                            entry.id = id;
                                            output[sc] = entry;
                                        }
                                        if (input.currentLevel >= 3)
                                        {
                                            if (sc == input.sceneId)
                                            {
                                                if (input.currentLevel == 3)
                                                    output = new MenuEntryData[testStructure.suites[su].types[ty].scenes[sc].tests.Count];
                                                for (int te = 0; te < testStructure.suites[su].types[ty].scenes[sc].tests.Count; te++)
                                                {
                                                    if (input.currentLevel == 3 && sc == input.sceneId)
                                                    {
                                                        MenuEntryData entry = new MenuEntryData();
                                                        MenuTestEntry id = Menu.Instance.CloneMenuID(input);
                                                        id.currentLevel = 3;                            // Replace relevent data 
                                                        id.suiteId = su;                                // Replace relevent data 
                                                        id.typeId = ty;                                 // Replace relevent data 
                                                        id.sceneId = sc;                                 // Replace relevent data 
                                                        id.testId = te;                                 // Replace relevent data 
                                                        entry.entryName = testStructure.suites[su].types[ty].scenes[sc].tests[te].testName;
                                                        entry.selectionState = testStructure.suites[su].types[ty].scenes[sc].tests[te].selectionState;
                                                        entry.id = id;
                                                        output[te] = entry;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Returned entry list for level " + input.currentLevel); // Write to console
            return output;
        }

        // Gets the name of a specific entry in the test structure
        public string GetNameOfEntry(int level, int suiteIndex, int typeIndex, int sceneIndex, int testIndex) // TODO - Move this to MenuEntryData input
        {
            string output = null;
            switch(level)
            {
                case 0:     // Suite
                    output = testStructure.suites[suiteIndex].suiteName;
                    break;
                case 1:     // Type
                    output = testStructure.suites[suiteIndex].types[typeIndex].typeName;
                    break;
                case 2:     // Scene
                    output = testStructure.suites[suiteIndex].types[typeIndex].scenes[sceneIndex].sceneName;
                    break;
                case 3:     // Test
                    output = testStructure.suites[suiteIndex].types[typeIndex].scenes[sceneIndex].tests[testIndex].testName;
                    break;
            }
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Returned name of entry " + output); // Write to console
            return output;
        }

        // ------------------------------------------------------------------------------------
        // Set Selection
        // - TODO - Clean and comment this
        
        public void SetSelectionState(MenuEntryData entryData)
        {
            switch (entryData.id.currentLevel)
            {
                case 0:     // Suite
                    testStructure.suites[entryData.id.suiteId].selectionState = entryData.selectionState;
                    break;
                case 1:     // Type
                    testStructure.suites[entryData.id.suiteId].types[entryData.id.typeId].selectionState = entryData.selectionState;
                    break;
                case 2:     // Scene
                    testStructure.suites[entryData.id.suiteId].types[entryData.id.typeId].scenes[entryData.id.sceneId].selectionState = entryData.selectionState;
                    break;
                case 3:     // Test
                    testStructure.suites[entryData.id.suiteId].types[entryData.id.typeId].scenes[entryData.id.sceneId].tests[entryData.id.testId].selectionState = entryData.selectionState;
                    break;
            }
            SetSelectionStateOnChildren(entryData);
            SetSelectionStateOnParents(entryData);
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Changed selection state for entry " + entryData.entryName); // Write to console
        }

        // Master class for setting selection state recursively down the hierarchy. Calls sub-function per level for every necessary level
        void SetSelectionStateOnChildren(MenuEntryData entryData)
        {
            switch (entryData.id.currentLevel)
            {
                case 0:     // Suite
                    SetSelectionOfLevelDownward(entryData, 0);
                    break;
                case 1:     // Type
                    SetSelectionOfLevelDownward(entryData, 1);
                    break;
                case 2:     // Scene
                    SetSelectionOfLevelDownward(entryData, 2);
                    break;
            }
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Changed selection state for all children of entry " + entryData.entryName); // Write to console
        }

        // Set a selection state of a specific level based on its immediate children 
        void SetSelectionOfLevelDownward(MenuEntryData entryData, int setLevel) // TODO - Can we shorten this?
        {
            switch (setLevel)
            {
                case 0:     // Suite
                    for (int ty = 0; ty < testStructure.suites[entryData.id.suiteId].types.Count; ty++)
                    {
                        testStructure.suites[entryData.id.suiteId].types[ty].selectionState = entryData.selectionState;
                        for (int sc = 0; sc < testStructure.suites[entryData.id.suiteId].types[ty].scenes.Count; sc++)
                        {
                            testStructure.suites[entryData.id.suiteId].types[ty].scenes[sc].selectionState = entryData.selectionState;
                            for (int te = 0; te < testStructure.suites[entryData.id.suiteId].types[ty].scenes[sc].tests.Count; te++)
                            {
                                testStructure.suites[entryData.id.suiteId].types[ty].scenes[sc].tests[te].selectionState = entryData.selectionState;
                            }
                        }
                    }
                    break;
                case 1:     // Type
                    for (int sc = 0; sc < testStructure.suites[entryData.id.suiteId].types[entryData.id.typeId].scenes.Count; sc++)
                    {
                        testStructure.suites[entryData.id.suiteId].types[entryData.id.typeId].scenes[sc].selectionState = entryData.selectionState;
                        for (int te = 0; te < testStructure.suites[entryData.id.suiteId].types[entryData.id.typeId].scenes[sc].tests.Count; te++)
                        {
                            testStructure.suites[entryData.id.suiteId].types[entryData.id.typeId].scenes[sc].tests[te].selectionState = entryData.selectionState;
                        }
                    }
                    break;
                case 2:     // Scene
                    for (int i = 0; i < testStructure.suites[entryData.id.suiteId].types[entryData.id.typeId].scenes[entryData.id.sceneId].tests.Count; i++)
                    {
                        testStructure.suites[entryData.id.suiteId].types[entryData.id.typeId].scenes[entryData.id.sceneId].tests[i].selectionState = entryData.selectionState;
                    }
                    break;
            }
        }

        // Master class for setting selection state recursively up the hierarchy. Calls sub-function per level for every necessary level
        void SetSelectionStateOnParents(MenuEntryData entryData)
        {
            switch (entryData.id.currentLevel)
            {
                case 1:     // Type
                    SetSelectionOfLevelUpward(entryData, 0);
                    break;
                case 2:     // Scene
                    SetSelectionOfLevelUpward(entryData, 1);
                    SetSelectionOfLevelUpward(entryData, 0);
                    break;
                case 3:     // Test
                    SetSelectionOfLevelUpward(entryData, 2);
                    SetSelectionOfLevelUpward(entryData, 1);
                    SetSelectionOfLevelUpward(entryData, 0);
                    break;
            }
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Changed selection state for all parents of entry " + entryData.entryName); // Write to console
        }

        // Set a selection state of a specific level based on its immediate children 
        void SetSelectionOfLevelUpward(MenuEntryData entryData, int setLevel) // TODO - Can we shorten this?
        {
            int x = 0;
            switch (setLevel)
            {
                case 0:     // Suite
                    for(int i = 0; i < testStructure.suites[entryData.id.suiteId].types.Count; i++)
                    {
                        if(testStructure.suites[entryData.id.suiteId].types[i].selectionState == 2)
                            break;
                        else if (testStructure.suites[entryData.id.suiteId].types[i].selectionState == 0)
                            x--;
                        else
                            x++;
                    }
                    if (x == testStructure.suites[entryData.id.suiteId].types.Count)
                        testStructure.suites[entryData.id.suiteId].selectionState = 1;
                    else if (x == -testStructure.suites[entryData.id.suiteId].types.Count)
                        testStructure.suites[entryData.id.suiteId].selectionState = 0;
                    else
                        testStructure.suites[entryData.id.suiteId].selectionState = 2;
                    break;
                case 1:     // Type
                    for (int i = 0; i < testStructure.suites[entryData.id.suiteId].types[entryData.id.typeId].scenes.Count; i++)
                    {
                        if (testStructure.suites[entryData.id.suiteId].types[entryData.id.typeId].scenes[i].selectionState == 2)
                            break;
                        else if (testStructure.suites[entryData.id.suiteId].types[entryData.id.typeId].scenes[i].selectionState == 0)
                            x--;
                        else
                            x++;
                    }
                    if (x == testStructure.suites[entryData.id.suiteId].types[entryData.id.typeId].scenes.Count)
                        testStructure.suites[entryData.id.suiteId].types[entryData.id.typeId].selectionState = 1;
                    else if (x == -testStructure.suites[entryData.id.suiteId].types[entryData.id.typeId].scenes.Count)
                        testStructure.suites[entryData.id.suiteId].types[entryData.id.typeId].selectionState = 0;
                    else
                        testStructure.suites[entryData.id.suiteId].types[entryData.id.typeId].selectionState = 2;
                    break;
                case 2:     // Scene
                    for (int i = 0; i < testStructure.suites[entryData.id.suiteId].types[entryData.id.typeId].scenes[entryData.id.sceneId].tests.Count; i++)
                    {
                        if (testStructure.suites[entryData.id.suiteId].types[entryData.id.typeId].scenes[entryData.id.sceneId].tests[i].selectionState == 2)
                            break;
                        else if (testStructure.suites[entryData.id.suiteId].types[entryData.id.typeId].scenes[entryData.id.sceneId].tests[i].selectionState == 0)
                            x--;
                        else
                            x++;
                    }
                    if (x == testStructure.suites[entryData.id.suiteId].types[entryData.id.typeId].scenes[entryData.id.sceneId].tests.Count)
                        testStructure.suites[entryData.id.suiteId].types[entryData.id.typeId].selectionState = 1;
                    else if (x == -testStructure.suites[entryData.id.suiteId].types[entryData.id.typeId].scenes[entryData.id.sceneId].tests.Count)
                        testStructure.suites[entryData.id.suiteId].types[entryData.id.typeId].scenes[entryData.id.sceneId].selectionState = 0;
                    else
                        testStructure.suites[entryData.id.suiteId].types[entryData.id.typeId].scenes[entryData.id.sceneId].selectionState = 2;
                    break;
            }
        }

        // ------------------------------------------------------------------------------------
        // Local Data Structures

        [Serializable]
        public class Structure
        {
            public List<Suite> suites = new List<Suite>();
        }

        [Serializable]
        public class Suite
        {
            public string suiteName;
            public int selectionState;
            public bool baseline;
            public List<TestType> types = new List<TestType>();
        }

        [Serializable]
        public class TestType
        {
            public string typeName;
            public int typeIndex;
            public int selectionState;
            public bool baseline;
            public List<Scene> scenes = new List<Scene>();
        }

        [Serializable]
        public class Scene
        {
            public string sceneName;
            public string scenePath;
            public int selectionState;
            public bool baseline;
            public List<Test> tests = new List<Test>();
        }

        [Serializable]
        public class Test
        {
            public string testName;
            public bool baseline;
            public int selectionState;
        }
    }
}
