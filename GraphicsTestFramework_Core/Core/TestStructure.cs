using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GraphicsTestFramework
{
    public class TestStructure : MonoBehaviour
    {
        public Structure testStructure;

        private bool levelWasLoaded = false;
        private void OnLevelWasLoaded(int iLevel)
        {
            levelWasLoaded = true;
        }

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

        private bool m_IsGenerated = false;
        public bool IsGenerated
        {
            get { return m_IsGenerated; }
        }

        void StartGeneration()
        {
            ProgressScreen.Instance.SetState(true, ProgressType.LocalLoad, "Generating Test Structure");
            StartCoroutine(GenerateStructure());
        }

        void OnEnable()
        {
            ResultsIO.baselinesParsed += StartGeneration;
        }

        //Desubscribe from event delegates
        void OnDisable()
        {
            ResultsIO.baselinesParsed -= StartGeneration;
        }

        public RunnerType UpdateStructure()
        {
            bool hasBaslines = CheckForBaselines();
            if(hasBaslines)
            {
                return RunnerType.Default;
            }
            else
            {
                return RunnerType.ResolveBaseline;
            }
        }

        /// ------------------------------------------------------------------------------------
        /// Generate test structure from SuiteManagers
        /// ------------------------------------------------------------------------------------
        /// - Loads all scenes and gets data from test lists
        /// - Reorganises for menu layout

        // TODO - Scenes with duplicate types between scenes do not merge at type level

        IEnumerator GenerateStructure()
        {
            testStructure = new Structure();
            List<Type> modelList = Common.GetSubTypes<TestModel>();
            for (int a = 0; a < SuiteManager.Instance.suites.Count; a++)
            {
                Suite newSuite = new Suite();
                string suiteName = SuiteManager.Instance.suites[a].suiteName;
                newSuite.suiteName = suiteName;
                for (int b = 0; b < SuiteManager.Instance.suites[a].scenes.Count; b++)
                {
                    SceneManager.LoadSceneAsync(SuiteManager.Instance.suites[a].scenes[b].path);
                    while (!levelWasLoaded)
                        yield return null;
                    levelWasLoaded = false;
                    UnityEngine.SceneManagement.Scene scene = SceneManager.GetSceneAt(0);
                    string sceneName = scene.name;
                    TestList testList = FindObjectOfType<TestList>();
                    for (int c = 0; c < testList.testTypes.Count; c++)
                    {
                        TestModel model = (TestModel)testList.testTypes[c].tests[0].testObject.GetComponent(modelList[testList.testTypes[c].testType]);
                        TestLogicBase logic = model.GetLogic();
                        model.SetLogic();
                        logic.SetName();
                        string typeName = logic.testTypeName;
                        TestType newType = FindDuplicateTypeInSuite(newSuite, typeName);
                        if(newType == null)
                        {
                            newType = new TestType();
                            newType.typeName = typeName;
                            newType.typeIndex = testList.testTypes[c].testType;
                            newSuite.types.Add(newType);
                            GenerateTestRunnerEntry(model);
                        }
                        Scene newScene = FindDuplicateSceneInType(newSuite, newType, sceneName);
                        if (newScene == null)
                        {
                            newScene = new Scene();
                            newScene.sceneName = sceneName;
                            newScene.scenePath = scene.path;
                            for (int d = 0; d < testList.testTypes[c].tests.Count; d++)
                            {
                                Test newTest = new Test();
                                newTest.testName = testList.testTypes[c].tests[d].testName;
                                newScene.tests.Add(newTest);
                            }
                            newType.scenes.Add(newScene);
                        }
                    }
                }
                testStructure.suites.Add(newSuite);
            }
            m_IsGenerated = true;
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log("Test Structure finished generating");
            ProgressScreen.Instance.SetState(false, ProgressType.LocalLoad, "");
        }

        void GenerateTestRunnerEntry(TestModel model)
        {
            if (Master.Instance.transform.Find("TestRunners"))
            {
                Transform runnerParent = Master.Instance.transform.Find("TestRunners");
                string childName = model.logic.ToString().Replace("GraphicsTestFramework.", "").Replace("Logic", "");
                if (!runnerParent.Find(childName))
                {
                    GameObject newChild = new GameObject();
                    newChild.transform.SetParent(runnerParent);
                    newChild.name = childName;
                    TestLogicBase logic = (TestLogicBase)newChild.AddComponent(model.logic);
                    logic.SetDisplayType();
                    logic.SetResultsType();
                    TestDisplayBase display = (TestDisplayBase)newChild.AddComponent(logic.displayType);
                    display.SetLogic(logic);
                    display.GetResultsContextObject();
                    TestTypeManager.Instance.AddType(logic);
                }
            }
            else
                Debug.LogError("Test Runner parent not found! Aborting");
        }
        
        bool CheckForBaselines()
        {
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
            for (int b = 0; b < suite.types.Count; b++)
            {
                if (suite.types[b].typeName == name)
                    return suite.types[b];
            }
            return null;
        }

        Scene FindDuplicateSceneInType(Suite suite, TestType type, string sceneName)
        {
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

        /// ------------------------------------------------------------------------------------
        /// Get data from Test Structure
        /// ------------------------------------------------------------------------------------
        /// - Used by menus to get data from the test structure
        
        // Get the entire list
        public Structure GetStructure()
        {
            return testStructure;
        }

        // Get a list of entries at a specific level
        public MenuEntryData[] GetEntries(TestID input) // TODO - Can we shorten this?
        {
            MenuEntryData[] output = null;

            if (input.currentLevel == 0)
                output = new MenuEntryData[testStructure.suites.Count];
            for (int su = 0; su < testStructure.suites.Count; su++)
            {
                if (input.currentLevel == 0)
                {
                    MenuEntryData entry = new MenuEntryData();
                    TestID id = Menu.Instance.CloneMenuID(input);
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
                                TestID id = Menu.Instance.CloneMenuID(input);
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
                                            TestID id = Menu.Instance.CloneMenuID(input);
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
                                                        TestID id = Menu.Instance.CloneMenuID(input);
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
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log("Test Structure returned entry list for level " + input.currentLevel);
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
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log("Test Structure returned name of entry " + output);
            return output;
        }

        // Checks whether any tests are selected (true if >0 tests are selected)
        public bool CheckSelectionNotNull()
        {
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log("Test Structure is checking for null selection");
            for (int su = 0; su < testStructure.suites.Count; su++)
            {
                if (testStructure.suites[su].selectionState != 0)
                    return true;
            }
            return false;
        }

        /// ------------------------------------------------------------------------------------
        /// Change selection states on Test Structure
        /// ------------------------------------------------------------------------------------
        /// - Used by menus to set selections
        
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
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log("Test Structure changed selection state for entry " + entryData.entryName);
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
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log("Test Structure changed selection state for all children of entry " + entryData.entryName);
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
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log("Test Structure changed selection state for all parents of entry " + entryData.entryName);
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

        /// ------------------------------------------------------------------------------------
        /// Data Structures

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
