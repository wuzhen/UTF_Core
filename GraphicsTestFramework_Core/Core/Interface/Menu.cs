using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GraphicsTestFramework
{
    public class Menu : MonoBehaviour
    {
        private static Menu _Instance = null;
        public static Menu Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = (Menu)FindObjectOfType(typeof(Menu));
                return _Instance;
            }
        }

        //public enum MenuState {Default, Baseline}
        //public MenuState menuState;

        public List list;
        public Breadcrumb breadcrumb;
        public Actions actions;
        public ResolveWindow resolveWindow;
        public Color[] colors;
        public GameObject menuListEntryPrefab;
        public GameObject listEntryPrefab;

        public MenuTestEntry selectedId;
        MenuListEntry selectedTest;
        float entryHeight;

        /// ------------------------------------------------------------------------------------
        /// First time setup

        private void Start()
        {
            // TODO - Need active on start for these singletons. Clean this up.
            TestViewer.Instance.SetTestViewerState(0, ViewerType.Default, null); 

            StartCoroutine(WaitForTestStructure());
        }

        // Have to wait for Test Structure to generate before generating menus (TODO - move this to a delegate)
        IEnumerator WaitForTestStructure()
        {
            do { yield return null; } while (!TestStructure.Instance.IsGenerated);
            // Check if need to force baseline resolution?
            if(TestStructure.Instance.CheckForBaselines()) // Baselines exist
            {
                SetupHomeButton();
                GenerateList();
                SetupActions();
            }
            else // Any or all baselines are missing
            {
                GenerateResolveList();
            }
        }

        /// ------------------------------------------------------------------------------------
        /// Update when returning to menu

        void UpdateMenu()
        {
            if (!TestStructure.Instance.CheckForBaselines())
            {
                GenerateResolveList();
            }
            else
            {
                if (selectedId.currentLevel == 4) // TODO - This is a dirty fix for returning to menu from view mode 
                                                  // returning the level of the selected test therefore with no items
                    selectedId.currentLevel = 3;
                ClearList();
                GenerateList();
            }
        }

        /// ------------------------------------------------------------------------------------
        /// Resolve Menu
        /// - Overrides main menu
        /// - Used for resolving problems with test structure
        ///     - Missing baselines
        
        void GenerateResolveList()
        {
            GenerateTestRunner(RunnerType.Resolve);
            ProgressScreen.Instance.SetState(false, ProgressType.LocalLoad, "");
            EnableResolveMenu();
        }

        void EnableResolveMenu()
        {
            resolveWindow.parent.SetActive(true);
            resolveWindow.message.text = TestRunner.Instance.runner.tests.Count+" baseline files are missing. This situation must be resolved before continuing. Manually resolve baselines for the listed tests.";
            float entryHeight = 0;
            for(int i = 0; i < TestRunner.Instance.runner.tests.Count; i++)
            {
                TestEntry currentTest = TestRunner.Instance.runner.tests[i];
                GameObject go = Instantiate(listEntryPrefab, resolveWindow.contentRect, false);
                RectTransform goRect = go.GetComponent<RectTransform>();
                goRect.anchoredPosition = new Vector2(0, entryHeight);
                ListEntry newEntry = go.GetComponent<ListEntry>();
                newEntry.Setup(currentTest.suiteName+" - "+currentTest.sceneName, currentTest.typeName+" - "+currentTest.testName);
                entryHeight -= goRect.sizeDelta.y;
            }
            resolveWindow.contentRect.sizeDelta = new Vector2(resolveWindow.contentRect.sizeDelta.x, -entryHeight);
        }

        void CleanupResolveMenu()
        {
            int listCount = resolveWindow.contentRect.transform.childCount;
            for(int i = 0; i < listCount; i++)
            {
                Destroy(resolveWindow.contentRect.GetChild(i).gameObject);
            }
            resolveWindow.parent.SetActive(false);
        }

        public void OnClickResolve()
        {
            CleanupResolveMenu();
            TestRunner.Instance.StartTests();
        }

        /// ------------------------------------------------------------------------------------
        /// List Management

        public void GenerateList()
        {
            // HP - Work out if locking everything out due to lacking baseline around here somewhere
            // HP - Set menu items red and force to resolve somehow (or skip menu altogether?)
            // HP - Build an automatic test runner from missing baseline
            MenuEntryData[] itemList = null;
            itemList = TestStructure.Instance.GetEntries(selectedId);
            if (itemList != null)
            {
                if (Master.Instance.debugMode == Master.DebugMode.Messages)
                    Debug.Log("Menu is generating list");
                for (int i = 0; i < itemList.Length; i++)
                {
                    GameObject go = Instantiate(menuListEntryPrefab, list.contentRect, false);
                    list.entryList.Add(go);
                    RectTransform goRect = go.GetComponent<RectTransform>();
                    goRect.anchoredPosition = new Vector2(0, entryHeight);
                    MenuListEntry newEntry = go.GetComponent<MenuListEntry>();
                    newEntry.Setup(itemList[i]);
                    entryHeight -= goRect.sizeDelta.y;
                }
                list.contentRect.sizeDelta = new Vector2(list.contentRect.sizeDelta.x, -entryHeight);
                if (Master.Instance.debugMode == Master.DebugMode.Messages)
                    Debug.Log("Menu finished generating list");
            }
            else
                Debug.LogWarning("Failed to retrieve Item List");
        }

        public void ClearList()
        {
            entryHeight = 0;
            foreach (GameObject go in list.entryList)
                Destroy(go);
            list.entryList.Clear();
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log("Menu finished clearing list");
        }

        /// ------------------------------------------------------------------------------------
        /// Breadcrumb

        // Setup the breadcrumbs home button
        public void SetupHomeButton()
        {
            breadcrumb.home.SetupHome();
        }

        // Generate the breadcrumb
        public void GenerateBreadcrumb(MenuEntryData inputData)
        {
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log("Menu is generating breadcrumb");
            switch(inputData.id.currentLevel)
            {
                case -1:     // Home
                    breadcrumb.suite.Setup(inputData, 1, 0);
                    breadcrumb.type.Setup(inputData, 1, 1);
                    breadcrumb.scene.Setup(inputData, 1, 2);
                    break;
                case 0:     // Suite
                    breadcrumb.suite.Setup(inputData, 0, 0);
                    breadcrumb.type.Setup(inputData, 1, 1);
                    breadcrumb.scene.Setup(inputData, 1, 2);
                    break;
                case 1:     // Type
                    breadcrumb.suite.Setup(inputData, 2, 0);
                    breadcrumb.type.Setup(inputData, 0, 1);
                    breadcrumb.scene.Setup(inputData, 1, 2);
                    break;
                case 2:     // Scene
                    breadcrumb.suite.Setup(inputData, 2, 0);
                    breadcrumb.type.Setup(inputData, 2, 1);
                    breadcrumb.scene.Setup(inputData, 0, 2);
                    break;
            }
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log("Menu finished generating breadcrumb");
        }

        /// ------------------------------------------------------------------------------------
        /// Navigation

        public void SetMenuState(int state)
        {
            switch(state)
            {
                case 0: // off
                    this.gameObject.SetActive(false);
                    break;
                case 1:
                    this.gameObject.SetActive(true);
                    UpdateMenu();
                    break;
            }
        }

        // Called every time a list entry is clicked (button listener)
        public void OnListEntryClick(MenuListEntry clicked)
        {
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log("Clicked menu entry for "+ clicked.entryData.entryName);
            if (clicked.entryData.id.currentLevel < 3) // Selecting any level with children
            {
                ClearList();
                GenerateBreadcrumb(clicked.entryData);
                MenuTestEntry id = CloneMenuID(clicked.entryData.id);
                id.currentLevel++;
                selectedId = id;
                GenerateList();
                CheckRunButtonStatus();
            }
            else // Selecting a single test (no children)
            {
                MenuTestEntry id = CloneMenuID(clicked.entryData.id);
                id.currentLevel++;
                selectedId = id;
                if (selectedTest != null)
                    selectedTest.mainButton.GetComponent<Image>().color = colors[2];
                selectedTest = clicked;
                selectedTest.mainButton.GetComponent<Image>().color = colors[1];
                CheckViewButtonStatus();
            }
        }

        // Called every time a breadcrumb entry is clicked (button listener)
        public void OnBreadcrumbEntryClick(MenuBreadcrumbEntry clicked)
        {
            MenuEntryData entryData = CloneMenuEntryData(clicked.entryData);
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log("Clicked breadcrumb entry for " + entryData.entryName);
            ClearList();
            selectedId = entryData.id;
            GenerateBreadcrumb(clicked.entryData);
            selectedId.currentLevel++;
            GenerateList();
            CheckViewButtonStatus();
        }

        /// ------------------------------------------------------------------------------------
        /// Actions
        
        void SetupActions()
        {
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log("Menu finished setting up actions panel");
            actions.run.onClick.AddListener(delegate { OnRunButtonClick(); });
            actions.view.onClick.AddListener(delegate { OnViewButtonClick(); });
        }
        
        // Check status of run button and update (called when changing a selection via toggles)
        public void CheckRunButtonStatus()
        {
            if (TestStructure.Instance.CheckSelectionNotNull())
                actions.run.interactable = true;
            else
                actions.run.interactable = false;
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log("Menu checked run button status");
        }

        // Check status of view button and update (called when navigating breadcrumb and selecting at level4 (test))
        public void CheckViewButtonStatus()
        {
            if (selectedId.currentLevel == 4)
                actions.view.interactable = true;
            else
            {
                actions.view.interactable = false;
                selectedTest = null;
            }
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log("Menu checked view button status");
        }

        // Called when run button is clicked
        public void OnRunButtonClick()
        {
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log("Clicked run button");
            GenerateTestRunner(RunnerType.Automation);
        }

        // Called when view button is clicked
        public void OnViewButtonClick()
        {
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log("Clicked view button");
            GenerateTestRunner(RunnerType.Manual);
        }

        /// ------------------------------------------------------------------------------------
        /// Execution

        // Generate and execute a new test runner
        void GenerateTestRunner(RunnerType type)
        {
            TestRunner newRunner;
            if (!Master.Instance.gameObject.GetComponent<TestRunner>())
                newRunner = Master.Instance.gameObject.AddComponent<TestRunner>();
            else
            {
                newRunner = Master.Instance.gameObject.GetComponent<TestRunner>();
                Debug.LogWarning("Test runner was not removed from previous iteration");
            }
            newRunner.SetupRunner(type);
        }

        /// ------------------------------------------------------------------------------------
        /// Data Helpers

        public MenuTestEntry CloneMenuID(MenuTestEntry input)
        {
            MenuTestEntry output = new MenuTestEntry();
            output.suiteId = input.suiteId;
            output.typeId = input.typeId;
            output.sceneId = input.sceneId;
            output.testId = input.testId;
            output.currentLevel = input.currentLevel;
            return output;
        }

        public MenuEntryData CloneMenuEntryData(MenuEntryData input)
        {
            MenuEntryData output = new MenuEntryData();
            output.entryName = input.entryName;
            output.selectionState = input.selectionState;
            output.id = CloneMenuID(input.id);
            return output;
        }

        public string GetLevelName(int index)
        {
            switch (index)
            {
                case 0:
                    return "Suite";
                case 1:
                    return "Type";
                case 2:
                    return "Scene";
                case 3:
                    return "Test";
            }
            return "";
        }

        public Color GetColor(int index)
        {
            return colors[index];
        }

        // Get the selected test. Used to start test runner at specific point when viewing tests
        public MenuTestEntry GetSelectedEntry()
        {
            return selectedTest.entryData.id;
        }

        /// ------------------------------------------------------------------------------------
        /// Member Data Structures

        [Serializable]
        public class List
        {
            public ScrollRect scrollRect;
            public RectTransform contentRect;
            public List<GameObject> entryList = new List<GameObject>();
        }

        [Serializable]
        public class Breadcrumb
        {
            public MenuBreadcrumbEntry home;
            public MenuBreadcrumbEntry suite;
            public MenuBreadcrumbEntry type;
            public MenuBreadcrumbEntry scene;
        }

        [Serializable]
        public class Actions
        {
            public Button run;
            public Button view;
        }

        [Serializable]
        public class ResolveWindow
        {
            public GameObject parent;
            public Text message;
            public RectTransform contentRect;
            public Button resolveButton;
        }
    }

    /// ------------------------------------------------------------------------------------
    /// Public Data Structures

    [Serializable]
    public class MenuTestEntry
    {
        public int suiteId = -1;
        public int typeId = -1;
        public int sceneId = -1;
        public int testId = -1;
        public int currentLevel = 0;
    }
}
