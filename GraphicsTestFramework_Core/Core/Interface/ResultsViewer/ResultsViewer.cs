using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

        // Main
        int viewerState; // Track state
        public GameObject resultsViewerParent;
        List<TestEntry> filteredResultsEntries = new List<TestEntry>();
        // Home buttons
        public Button homeButton;
        public Button overviewButton;
        // Filters
        public Dropdown suitesDropdown;
        public Dropdown typesDropdown;
        public Dropdown resultsDropdown;
        // Overview
        public GameObject overviewParent;
        public OverviewResultsWindow overviewResultsWindow;
        public Image piePass;
        public Image pieFail;
        // Detailed results
        public GameObject detailedResultsParent;
        public RectTransform listContentRect;
        public GameObject resultsEntryPrefab;
        List<GameObject> listEntries = new List<GameObject>();
        float entryHeight;
        // Context entry
        ResultsEntry activeContextEntry;
        GameObject activeContextObject;

        // ------------------------------------------------------------------------------------
        // State

        // Initial setup
        public void Setup()
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Setup"); // Write to console
            GenerateSuitesDropdown(); // Generate suite dropdown
            GenerateTypesDropdown(); // Generate types dropdown
            GenerateResultsDropdown(); // Generate results dropdown
        }

        // Set the UI state
        public void SetState(int input)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Setting state"); // Write to console
            viewerState = input; // Track state
            switch(viewerState) // Switch UI objects based on state
            {
                case 0: // Back to menu
                    overviewParent.SetActive(false);
                    detailedResultsParent.SetActive(false);
                    resultsViewerParent.SetActive(false);
                    homeButton.gameObject.SetActive(false);
                    overviewButton.gameObject.SetActive(false);
                    //SetResultsDropdownState(false);
                    break;
                case 1: // Overview
                    overviewParent.SetActive(true);
                    detailedResultsParent.SetActive(false);
                    resultsViewerParent.SetActive(true);
                    homeButton.gameObject.SetActive(true);
                    overviewButton.gameObject.SetActive(false);
                    //SetResultsDropdownState(false);
                    break;
                case 2: // Detailed Results
                    overviewParent.SetActive(false);
                    detailedResultsParent.SetActive(true);
                    resultsViewerParent.SetActive(true);
                    homeButton.gameObject.SetActive(false);
                    overviewButton.gameObject.SetActive(true);
                    //SetResultsDropdownState(true);
                    break;
            }
            GenerateContent(); // Main call for generation of viewer content
        }

        // ------------------------------------------------------------------------------------
        // Filters

        // Get suite names and generate dropdown content
        void GenerateSuitesDropdown()
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Generating suites dropdown"); // Write to console
            List<Dropdown.OptionData> options = new List<Dropdown.OptionData>(); // Create new options list
            string[] suites = SuiteManager.GetSuiteNames(); // Get suite names
            for(int i = 0; i < suites.Length; i++) // Iterate suites
                options.Add(Common.ConvertStringToDropdownOptionData(suites[i])); // Convert string to option data and add
            suitesDropdown.AddOptions(options); // Add options
        }

        // Get type names and generate dropdown content
        void GenerateTypesDropdown()
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Generating types dropdown"); // Write to console
            List<Dropdown.OptionData> options = new List<Dropdown.OptionData>(); // Create new options list
            string[] types = TestTypes.GetTypeStringList(); // Get type names
            for (int i = 0; i < types.Length; i++) // Iterate types
                options.Add(Common.ConvertStringToDropdownOptionData(types[i])); // Convert string to option data and add
            typesDropdown.AddOptions(options); // Add options
        }

        // Generate results filter dropdown content
        void GenerateResultsDropdown()
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Generating results dropdown"); // Write to console
            List<Dropdown.OptionData> options = new List<Dropdown.OptionData>(); // Create new options list
            string[] results = new string[4] { "Pass", "Fail", "Ran", "Not Ran" }; // Get results entries
            for (int i = 0; i < results.Length; i++) // Iterate
                options.Add(Common.ConvertStringToDropdownOptionData(results[i])); // Convert string to option data and add
            resultsDropdown.AddOptions(options); // Add options
        }

        // Set results dropdown state based on ResultsViewer state
        void SetResultsDropdownState(bool input)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Settings results dropdown state"); // Write to console
            switch (input)
            {
                case true:
                    resultsDropdown.interactable = true; // Enable
                    break;
                case false:
                    resultsDropdown.value = 0; // Reset value
                    resultsDropdown.interactable = false; // Disable
                    break;
            }
        }

        // ------------------------------------------------------------------------------------
        // Content Main

        // Generate all content for detailed results based on filters
        public void GenerateContent()
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Generating content"); // Write to console
            switch (viewerState)
            {
                case 0: // Back to menud
                    break;
                case 1: // Overview
                    GenerateFilteredResultList(); // Generate filtered result list
                    GenerateOverview(); // Generate overview content
                    break;
                case 2: // Detailed Results
                    GenerateFilteredResultList(); // Generate filtered result list
                    GenerateDetailedResultsList(); // Generate detailed results list
                    break;
            }
        }

        // Generate a list of results based on selected filters
        public void GenerateFilteredResultList()
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Filtering results"); // Write to console
            filteredResultsEntries.Clear(); // Clear current
            TestStructure.Structure structure = TestStructure.Instance.GetStructure(); // Get structure
            for (int su = 0; su < structure.suites.Count; su++) // Iterate suites 
            {
                if (su == suitesDropdown.value - 1 || suitesDropdown.value == 0) // If passes suites filter
                {
                    string suiteName = structure.suites[su].suiteName; // Get suite name
                    for (int ty = 0; ty < structure.suites[su].types.Count; ty++) // Iterate types
                    {
                        int typeIndex = structure.suites[su].types[ty].typeIndex; // Get type index
                        if (typeIndex == typesDropdown.value - 1 || typesDropdown.value == 0) // If passes types filter
                        {
                            string typeName = structure.suites[su].types[ty].typeName; // Get type name
                            for (int gr = 0; gr < structure.suites[su].types[ty].groups.Count; gr++) // Iterate groups
                            {
                                string groupName = structure.suites[su].types[ty].groups[gr].groupName; // Get group name
                                for (int te = 0; te < structure.suites[su].types[ty].groups[gr].tests.Count; te++) // Iterate tests
                                {
                                    string testName = structure.suites[su].types[ty].groups[gr].tests[te].testName; // Get test name
                                    if (resultsDropdown.value != 0) // If filtering based on results
                                    {
                                        ResultsDataCommon common = BuildResultsDataCommon(groupName, testName); // Build results data common to retrieve results
                                        ResultsIOData data = ResultsIO.Instance.RetrieveResult(suiteName, typeName, common); // Retrieve results data
                                        int passFail = 2; // Set default state (no results)
                                        if (data != null) // If results data exists
                                            passFail = data.resultsRow[0].resultsColumn[21] == "True" ? 0 : 1; // Set pass fail state
                                        switch(resultsDropdown.value)
                                        {
                                            case 1: // Pass
                                                if(passFail == 0)
                                                    filteredResultsEntries.Add(new TestEntry(suiteName, groupName, "", typeName, testName, typeIndex, su, gr, ty, te)); // Add to list
                                                break;
                                            case 2: // Fail
                                                if (passFail == 1)
                                                    filteredResultsEntries.Add(new TestEntry(suiteName, groupName, "", typeName, testName, typeIndex, su, gr, ty, te)); // Add to list
                                                break;
                                            case 3: // Ran
                                                if (passFail != 2)
                                                    filteredResultsEntries.Add(new TestEntry(suiteName, groupName, "", typeName, testName, typeIndex, su, gr, ty, te)); // Add to list
                                                break;
                                            case 4: // Not Ran
                                                if (passFail == 2)
                                                    filteredResultsEntries.Add(new TestEntry(suiteName, groupName, "", typeName, testName, typeIndex, su, gr, ty, te)); // Add to list
                                                break;
                                        }
                                    }
                                    else
                                        filteredResultsEntries.Add(new TestEntry(suiteName, groupName, "", typeName, testName, typeIndex, su, gr, ty, te)); // Add to list
                                }
                            }
                        }
                    }
                }
            }
        }

        // ------------------------------------------------------------------------------------
        // Overview

        // Generate overview content
        void GenerateOverview()
        {
            int totalTests = filteredResultsEntries.Count; // Get total tests
            int testsPassed = 0; // Track tests passed
            int testsFailed = 0; // Track tests failed
            for (int i = 0; i < filteredResultsEntries.Count; i++) // Iterate filtered results
            {
                ResultsDataCommon common = BuildResultsDataCommon(filteredResultsEntries[i].groupName, filteredResultsEntries[i].testName); // Build results data common to retrieve results
                ResultsIOData data = ResultsIO.Instance.RetrieveResult(filteredResultsEntries[i].suiteName, filteredResultsEntries[i].typeName, common); // Retrieve results data
                int passFail = 2; // Set default state (no results)
                if (data != null) // If results data exists
                    passFail = data.resultsRow[0].resultsColumn[21] == "True" ? 0 : 1; // Set pass fail state
                switch(passFail)
                {
                    case 0: // Pass
                        testsPassed++; // Increment
                        break;
                    case 1: // Fail
                        testsFailed++; // Increment
                        break;
                }
            }
            int testsRan = testsPassed + testsFailed; // Get tests ran
            int testsNotRan = totalTests - testsRan; // Get tests not ran
            float passPercentage = Common.SafeDivision((float)testsPassed, (float)totalTests) * 100; // Get pass percentage
            float failPercentage = Common.SafeDivision((float)testsFailed, (float)totalTests) * 100; // Get fail percentage
            float ranPercentage = Common.SafeDivision((float)testsRan, (float)totalTests) * 100; // Get ran percentage
            float notRanPercentage = Common.SafeDivision((float)testsNotRan, (float)totalTests) * 100; // Get not ran percentage
            overviewResultsWindow.totalTests.text = totalTests.ToString(); // Set to UI
            overviewResultsWindow.testsRan.text = testsRan.ToString(); // Set to UI
            overviewResultsWindow.testsPassed.text = testsPassed.ToString(); // Set to UI
            overviewResultsWindow.testsFailed.text = testsFailed.ToString(); // Set to UI
            overviewResultsWindow.testsNotRan.text = testsNotRan.ToString(); // Set to UI
            overviewResultsWindow.passPercentage.text = passPercentage.ToString("0.##") + "%"; // Set to UI
            overviewResultsWindow.failPercentage.text = failPercentage.ToString("0.##") + "%"; // Set to UI
            overviewResultsWindow.ranPercentage.text = ranPercentage.ToString("0.##") + "%"; // Set to UI
            overviewResultsWindow.notRanPercentage.text = notRanPercentage.ToString("0.##") + "%"; // Set to UI
            piePass.fillAmount = passPercentage / 100; // Set pie value
            pieFail.fillAmount = (passPercentage / 100) + (failPercentage / 100); // Set pie value
        }

        // ------------------------------------------------------------------------------------
        // Detailed Results List

        // Generate main list
        void GenerateDetailedResultsList()
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Generating list"); // Write to console
            ClearDetailedResultsList(); // Clear current list
            DestroyContextEntry(); // Destroy the context object
            for(int i = 0; i < filteredResultsEntries.Count; i++) // Iterate filtered results
            {
                ResultsDataCommon common = BuildResultsDataCommon(filteredResultsEntries[i].groupName, filteredResultsEntries[i].testName); // Build results data common to retrieve results
                ResultsIOData data = ResultsIO.Instance.RetrieveResult(filteredResultsEntries[i].suiteName, filteredResultsEntries[i].typeName, common); // Retrieve results data
                TestLogicBase logic = TestTypeManager.Instance.GetLogicInstanceFromName(filteredResultsEntries[i].typeName); // Get logic instance
                GameObject go = Instantiate(resultsEntryPrefab, listContentRect, false); // Create results entry instance
                listEntries.Add(go); // Add to list
                RectTransform goRect = go.GetComponent<RectTransform>(); // Get rect
                goRect.anchoredPosition = new Vector2(0, entryHeight); // Set position
                ResultsEntry newEntry = go.GetComponent<ResultsEntry>(); // Get ResultsEntry reference
                newEntry.Setup(filteredResultsEntries[i].suiteName, common.GroupName, common.TestName, data, logic); // Setup the instance
                entryHeight -= goRect.sizeDelta.y; // Track height for next entry
            }
            listContentRect.sizeDelta = new Vector2(listContentRect.sizeDelta.x, -entryHeight); // Set content rect size
        }

        // Clear main list
        void ClearDetailedResultsList()
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Clearing list"); // Write to console
            foreach (GameObject go in listEntries) // Iterate current list entries
                Destroy(go); // Delete them
            listEntries.Clear(); // Clear the list
            entryHeight = 0; // Reset
        }

        // Find an entries index from its instance
        int FindEntryInDetailedResultsList(ResultsEntry inputEntry)
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
        void NudgeDetailedResultsListEntries(int startIndex, float nudgeAmount)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Nudging list entries from start index "+startIndex); // Write to console
            for (int i = startIndex+1; i < listEntries.Count; i++) // Iterate entries after the start index
            {
                RectTransform entryRect = listEntries[i].GetComponent<RectTransform>(); // Get rect reference
                entryRect.anchoredPosition = new Vector2(entryRect.anchoredPosition.x, entryRect.anchoredPosition.y + nudgeAmount); // Nudge the entry
            }
        }

        // ------------------------------------------------------------------------------------
        // Detailed Results Context Object

        // Toggle the context object
        public void ToggleContextObject(ResultsEntry inputEntry, TestDisplayBase display)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Toggling context object for " + inputEntry); // Write to console
            if (activeContextObject == null) // If context object is null
                ExpandContextObject(inputEntry, display); // Create and expand
            else
            {
                if(activeContextEntry == inputEntry) // If selected entry matches current context
                    HideContextObject(inputEntry); // Hide it
                else
                {
                    HideContextObject(activeContextEntry); // Hide the current
                    ExpandContextObject(inputEntry, display); // Create and expand
                }
            }
                
        }

        // Create and expand context object
        void ExpandContextObject(ResultsEntry inputEntry, TestDisplayBase display)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Expanding context object"); // Write to console
            int entryIndex = FindEntryInDetailedResultsList(inputEntry); // Get index of selected entry
            activeContextEntry = inputEntry; // Track selected entry
            activeContextObject = Instantiate(display.resultsContextPrefab, listContentRect, false); // Create context object instance
            RectTransform contextObjectRect = activeContextObject.GetComponent<RectTransform>(); // Get rect
            contextObjectRect.anchoredPosition = new Vector2(0, (entryIndex+1) * -listEntries[0].GetComponent<RectTransform>().sizeDelta.y); // Set position
            listContentRect.sizeDelta = new Vector2(listContentRect.sizeDelta.x, listContentRect.sizeDelta.y + contextObjectRect.sizeDelta.y); // Set size
            NudgeDetailedResultsListEntries(entryIndex, -contextObjectRect.sizeDelta.y); // Nudge entries
            display.SetupResultsContext(activeContextObject.GetComponent<ResultsContext>(), inputEntry); // Tell Display how to setup the results context
        }

        // Hide and destroy context object
        void HideContextObject(ResultsEntry inputEntry)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Hiding context object"); // Write to console
            int entryIndex = FindEntryInDetailedResultsList(inputEntry); // Get index of selected entry
            NudgeDetailedResultsListEntries(entryIndex, activeContextObject.GetComponent<RectTransform>().sizeDelta.y); // Nudge entries
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
        // Helper Functions

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

        // ------------------------------------------------------------------------------------
        // Local Data Structures

        [System.Serializable]
        public class OverviewResultsWindow
        {
            public Text totalTests;
            public Text testsRan;
            public Text testsPassed;
            public Text testsFailed;
            public Text testsNotRan;
            public Text passPercentage;
            public Text failPercentage;
            public Text ranPercentage;
            public Text notRanPercentage;
        }
    }
}
