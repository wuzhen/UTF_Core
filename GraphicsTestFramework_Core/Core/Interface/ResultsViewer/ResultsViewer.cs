using System.Collections.Generic;
using UnityEngine;

namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // ResultsViewer
    // - The main ResultsViewer controller
    // - Controls results viewer context
    // - Distributes context viewer operations to correct Display scripts

    public class ResultsViewer : MonoBehaviour
    {
        // ------------------------------------------------------------------------------------
        // Variables

        // Singleton
        private static ResultsViewer _Instance = null;
        public static ResultsViewer Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = (ResultsViewer)FindObjectOfType(typeof(ResultsViewer));
                return _Instance;
            }
        }

        // UI Object References
        public GameObject resultsViewerParent;
        public GameObject tabPrefab;
        public GameObject resultsEntryPrefab;
        public RectTransform suiteTabContentRect;
        public RectTransform typeTabContentRect;
        public RectTransform listContentRect;

        // Data
        List<GameObject> suiteTabs = new List<GameObject>();
        List<GameObject> typeTabs = new List<GameObject>();
        List<GameObject> listEntries = new List<GameObject>();
        int selectedSuite;
        int selectedType;
        float entryHeight;
        GameObject activeContextObject;

        // ------------------------------------------------------------------------------------
        // Navigation

        // Enable ResultsViewer
        public void Enable()
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Enabling results viewer"); // Write to console
            resultsViewerParent.SetActive(true); // Enable main parent object
            GenerateSuiteTabs(); // Generate suite tabs
        }

        // Disable ResultsViewer
        public void BackToMenu()
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Returning to menu"); // Write to console
            resultsViewerParent.SetActive(false); // Disable main parent object
        }

        // ------------------------------------------------------------------------------------
        // Tabs

        // Generate Suite tabs
        void GenerateSuiteTabs()
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Generating suite tabs"); // Write to console
            foreach (GameObject go in suiteTabs) // Iterate current suite tabs
                Destroy(go); // Delete them
            suiteTabs.Clear(); // Clear list
            int suiteCount = TestStructure.Instance.testStructure.suites.Count; // Get suite count from test structure
            for(int su = 0; su < suiteCount; su++) // Iterate suite count
            {
                GameObject go = Instantiate(tabPrefab, suiteTabContentRect, false); // Create instance
                suiteTabs.Add(go); // Add to list
                ResultsTab newResultsTab = go.GetComponent<ResultsTab>(); // Get results tab reference
                newResultsTab.Setup(ResultsTab.TabType.Suite, su, TestStructure.Instance.testStructure.suites[su].suiteName); // Setup the tab
            }
            SelectSuite(0); // Always select first suite
        }

        // Generate Suite tabs
        void GenerateTypeTabs()
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Generating type tabs"); // Write to console
            foreach (GameObject go in typeTabs) // Iterate current suite tabs
                Destroy(go); // Delete them
            typeTabs.Clear(); // Clear list
            int typeCount = TestStructure.Instance.testStructure.suites[selectedSuite].types.Count; // Get type count from test structure
            for (int ty = 0; ty < typeCount; ty++) // Iterate type count
            {
                GameObject go = Instantiate(tabPrefab, typeTabContentRect, false); // Create instance
                typeTabs.Add(go); // Add to list
                ResultsTab newResultsTab = go.GetComponent<ResultsTab>(); // Get results tab reference
                newResultsTab.Setup(ResultsTab.TabType.TestType, ty, TestStructure.Instance.testStructure.suites[selectedSuite].types[ty].typeName); // Setup the tab
            }
            SelectType(0); // Always select first type
        }

        // ------------------------------------------------------------------------------------
        // Selection

        // Select suite at index
        public void SelectSuite(int index)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Selecting suite at index "+index); // Write to console
            selectedSuite = index; // Set selected suite
            GenerateTypeTabs(); // Regenerate type tabs
        }

        // Select type at index
        public void SelectType(int index)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Selecting type at index " + index); // Write to console
            selectedType = index; // Set selected type
            GenerateList(); // Regenerate list
        }

        // ------------------------------------------------------------------------------------
        // List Management

        // Generate main list
        void GenerateList()
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Generating list"); // Write to console
            ClearList(); // Clear current list
            DestroyContextEntry(); // Destroy the context object
            int groupCount = TestStructure.Instance.testStructure.suites[selectedSuite].types[selectedType].groups.Count; // Get group count from test structure
            for (int gr = 0; gr < groupCount; gr++) // Iterate groups
            {
                string groupName = TestStructure.Instance.testStructure.suites[selectedSuite].types[selectedType].groups[gr].groupName; // Get group name from test structure
                int testCount = TestStructure.Instance.testStructure.suites[selectedSuite].types[selectedType].groups[gr].tests.Count; // Get test count from test structure
                for (int te = 0; te < testCount; te++) // Iterate tests
                {
                    string testName = TestStructure.Instance.testStructure.suites[selectedSuite].types[selectedType].groups[gr].tests[te].testName; // Get test name from test structure
                    ResultsDataCommon common = BuildResultsDataCommon(groupName, testName); // Build results data common to retrieve results
                    ResultsIOData data = ResultsIO.Instance.RetrieveResult(TestStructure.Instance.testStructure.suites[selectedSuite].suiteName, TestStructure.Instance.testStructure.suites[selectedSuite].types[selectedType].typeName, common); // Retrieve results data
                    TestLogicBase logic = TestTypeManager.Instance.GetLogicInstanceFromName(TestStructure.Instance.testStructure.suites[selectedSuite].types[selectedType].typeName); // Get logic instance
                    GameObject go = Instantiate(resultsEntryPrefab, listContentRect, false); // Create results entry instance
                    listEntries.Add(go); // Add to list
                    RectTransform goRect = go.GetComponent<RectTransform>(); // Get rect
                    goRect.anchoredPosition = new Vector2(0, entryHeight); // Set position
                    ResultsEntry newEntry = go.GetComponent<ResultsEntry>(); // Get ResultsEntry reference
                    newEntry.Setup(TestStructure.Instance.testStructure.suites[selectedSuite].suiteName, common.GroupName, common.TestName, data, logic); // Setup the instance
                    entryHeight -= goRect.sizeDelta.y; // Track height for next entry
                }
            }
            listContentRect.sizeDelta = new Vector2(listContentRect.sizeDelta.x, -entryHeight); // Set content rect size
        }

        // Clear main list
        void ClearList()
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Clearing list"); // Write to console
            foreach (GameObject go in listEntries) // Iterate current list entries
                Destroy(go); // Delete them
            listEntries.Clear(); // Clear the list
            entryHeight = 0; // Reset
        }

        // Find an entries index from its instance
        int FindEntryInList(ResultsEntry inputEntry)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Finding entry in list"); // Write to console
            for (int i = 0; i < listEntries.Count; i++) // Iterate current entries
            {
                if (listEntries[i] == inputEntry.gameObject) // If the entry matches
                    return i; // Return the index
            }
            return -1; // Return fail
        }

        // Nudge list entries up or down to create space for context object
        void NudgeListEntries(int startIndex, float nudgeAmount)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Nudging list entries from start index "+startIndex); // Write to console
            for (int i = startIndex+1; i < listEntries.Count; i++) // Iterate entries after the start index
            {
                RectTransform entryRect = listEntries[i].GetComponent<RectTransform>(); // Get rect reference
                entryRect.anchoredPosition = new Vector2(entryRect.anchoredPosition.x, entryRect.anchoredPosition.y + nudgeAmount); // Nudge the entry
            }
        }

        // ------------------------------------------------------------------------------------
        // Context Object

        // Toggle the context object
        public void ToggleContextObject(ResultsEntry inputEntry, TestDisplayBase display)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Toggling context object for " + inputEntry); // Write to console
            if (activeContextObject == null) // If context object is null
                ExpandContextObject(inputEntry, display); // Create and expand
            else
                HideContextObject(inputEntry); // Hide it
        }

        // Create and expand context object
        void ExpandContextObject(ResultsEntry inputEntry, TestDisplayBase display)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Expanding context object"); // Write to console
            int entryIndex = FindEntryInList(inputEntry); // Get index of selected entry
            activeContextObject = Instantiate(display.resultsContextPrefab, listContentRect, false); // Create context object instance
            RectTransform contextObjectRect = activeContextObject.GetComponent<RectTransform>(); // Get rect
            contextObjectRect.anchoredPosition = new Vector2(0, (entryIndex+1) * -listEntries[0].GetComponent<RectTransform>().sizeDelta.y); // Set position
            listContentRect.sizeDelta = new Vector2(listContentRect.sizeDelta.x, listContentRect.sizeDelta.y + contextObjectRect.sizeDelta.y); // Set size
            NudgeListEntries(entryIndex, -contextObjectRect.sizeDelta.y); // Nudge entries
            display.SetupResultsContext(activeContextObject.GetComponent<ResultsContext>(), inputEntry); // Tell Display how to setup the results context
        }

        // Hide and destroy context object
        void HideContextObject(ResultsEntry inputEntry)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Hiding context object"); // Write to console
            int entryIndex = FindEntryInList(inputEntry); // Get index of selected entry
            NudgeListEntries(entryIndex, activeContextObject.GetComponent<RectTransform>().sizeDelta.y); // Nudge entries
            listContentRect.sizeDelta = new Vector2(listContentRect.sizeDelta.x, listContentRect.sizeDelta.y - activeContextObject.GetComponent<RectTransform>().sizeDelta.y); // Set size
            DestroyContextEntry(); // Destroy
        }

        // Destroy context object
        void DestroyContextEntry()
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Destroying context object"); // Write to console
            if (activeContextObject) // If exists
            {
                Destroy(activeContextObject); // Destroy object
                activeContextObject = null; // Set null
            }
        }

        // ------------------------------------------------------------------------------------
        // Local Data Structures

        // TODO - Should this be global?
        ResultsDataCommon BuildResultsDataCommon(string sceneName, string testName)
        {
            ResultsDataCommon common = new ResultsDataCommon();
            SystemData systemData = Master.Instance.GetSystemData();
            common.API = systemData.API;
            common.RenderPipe = "Standard Legacy"; // TODO - Implement SRP support
            common.GroupName = sceneName;
            common.TestName = testName;
            return common;
        }
    }
}
