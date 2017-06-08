using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GraphicsTestFramework
{
    public class ResultsViewer : MonoBehaviour
    {
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

        public GameObject resultsViewerParent;
        public GameObject tabPrefab;
        public GameObject resultsEntryPrefab;
        public RectTransform suiteTabContentRect;
        public RectTransform typeTabContentRect;
        public RectTransform listContentRect;

        GameObject activeContextObject;

        int selectedSuite;
        int selectedType;
        List<GameObject> suiteTabs = new List<GameObject>();
        List<GameObject> typeTabs = new List<GameObject>();
        List<GameObject> listEntries = new List<GameObject>();
        float entryHeight;

        public void Enable()
        {
            resultsViewerParent.SetActive(true);
            GenerateSuiteTabs();
        }

        public void BackToMenu()
        {
            Cleanup();
            resultsViewerParent.SetActive(false);
        }

        void Cleanup()
        {
            // Probably need this
        }

        void GenerateSuiteTabs()
        {
            foreach (GameObject go in suiteTabs)
                Destroy(go);
            suiteTabs.Clear();
            int suiteCount = TestStructure.Instance.testStructure.suites.Count;
            for(int su = 0; su < suiteCount; su++)
            {
                GameObject go = Instantiate(tabPrefab, suiteTabContentRect, false);
                suiteTabs.Add(go);
                ResultsTab newResultsTab = go.GetComponent<ResultsTab>();
                newResultsTab.Setup(ResultsTab.TabType.Suite, su, TestStructure.Instance.testStructure.suites[su].suiteName);
            }
            SelectSuite(0);
        }

        public void SelectSuite(int index)
        {
            selectedSuite = index;
            GenerateTypeTabs();
        }

        void GenerateTypeTabs()
        {
            foreach (GameObject go in typeTabs)
                Destroy(go);
            typeTabs.Clear();
            int typeCount = TestStructure.Instance.testStructure.suites[selectedSuite].types.Count;
            for (int ty = 0; ty < typeCount; ty++)
            {
                GameObject go = Instantiate(tabPrefab, typeTabContentRect, false);
                typeTabs.Add(go);
                ResultsTab newResultsTab = go.GetComponent<ResultsTab>();
                newResultsTab.Setup(ResultsTab.TabType.TestType, ty, TestStructure.Instance.testStructure.suites[selectedSuite].types[ty].typeName);
            }
            SelectType(0);
        }

        public void SelectType(int index)
        {
            selectedType = index;
            GenerateList();
        }

        void GenerateList()
        {
            ClearList();
            int groupCount = TestStructure.Instance.testStructure.suites[selectedSuite].types[selectedType].groups.Count;
            for (int gr = 0; gr < groupCount; gr++)
            {
                string sceneName = TestStructure.Instance.testStructure.suites[selectedSuite].types[selectedType].groups[gr].groupName;
                int testCount = TestStructure.Instance.testStructure.suites[selectedSuite].types[selectedType].groups[gr].tests.Count;
                for (int te = 0; te < testCount; te++)
                {
                    string testName = TestStructure.Instance.testStructure.suites[selectedSuite].types[selectedType].groups[gr].tests[te].testName;
                    ResultsDataCommon common = BuildResultsDataCommon(sceneName, testName);
                    ResultsIOData data = ResultsIO.Instance.RetrieveResult(TestStructure.Instance.testStructure.suites[selectedSuite].suiteName, TestStructure.Instance.testStructure.suites[selectedSuite].types[selectedType].typeName, common);
                    TestLogicBase logic = TestTypeManager.Instance.GetLogicInstanceFromName(TestStructure.Instance.testStructure.suites[selectedSuite].types[selectedType].typeName);
                    GameObject go = Instantiate(resultsEntryPrefab, listContentRect, false);
                    listEntries.Add(go);
                    RectTransform goRect = go.GetComponent<RectTransform>();
                    goRect.anchoredPosition = new Vector2(0, entryHeight);
                    ResultsEntry newEntry = go.GetComponent<ResultsEntry>();
                    newEntry.Setup(common.SceneName, common.TestName, data, logic);
                    entryHeight -= goRect.sizeDelta.y;
                }
            }
            listContentRect.sizeDelta = new Vector2(listContentRect.sizeDelta.x, -entryHeight);
        }

        void ClearList()
        {
            foreach(GameObject go in listEntries)
            {
                Destroy(go);
            }
            listEntries.Clear();
            entryHeight = 0;
        }

        int FindEntryInList(ResultsEntry inputEntry)
        {
            for(int i = 0; i < listEntries.Count; i++)
            {
                if (listEntries[i] == inputEntry.gameObject)
                    return i;
            }
            return -1;
        }

        void NudgeListEntries(int startIndex, float nudgeAmount)
        {
            for(int i = startIndex+1; i < listEntries.Count; i++)
            {
                RectTransform entryRect = listEntries[i].GetComponent<RectTransform>();
                entryRect.anchoredPosition = new Vector2(entryRect.anchoredPosition.x, entryRect.anchoredPosition.y + nudgeAmount);
            }
        }

        public void ToggleContextObject(ResultsEntry inputEntry, TestDisplayBase display)
        {
            if (activeContextObject == null)
                ExpandContextObject(inputEntry, display);
            else
                HideContextObject(inputEntry);
        }

        void ExpandContextObject(ResultsEntry inputEntry, TestDisplayBase display)
        {
            int entryIndex = FindEntryInList(inputEntry);
            activeContextObject = Instantiate(display.resultsContextPrefab, listContentRect, false);
            RectTransform contextObjectRect = activeContextObject.GetComponent<RectTransform>();
            contextObjectRect.anchoredPosition = new Vector2(0, (entryIndex+1) * -listEntries[0].GetComponent<RectTransform>().sizeDelta.y);
            listContentRect.sizeDelta = new Vector2(listContentRect.sizeDelta.x, listContentRect.sizeDelta.y + contextObjectRect.sizeDelta.y);
            NudgeListEntries(entryIndex, -contextObjectRect.sizeDelta.y);
            display.SetupResultsContext(activeContextObject.GetComponent<ResultsContext>(), inputEntry);
        }

        void HideContextObject(ResultsEntry inputEntry)
        {
            int entryIndex = FindEntryInList(inputEntry);
            NudgeListEntries(entryIndex, activeContextObject.GetComponent<RectTransform>().sizeDelta.y);
            listContentRect.sizeDelta = new Vector2(listContentRect.sizeDelta.x, listContentRect.sizeDelta.y - activeContextObject.GetComponent<RectTransform>().sizeDelta.y);
            Destroy(activeContextObject);
            activeContextObject = null;
        }

        // TODO - This shouldnt be here
        ResultsDataCommon BuildResultsDataCommon(string sceneName, string testName)
        {
            ResultsDataCommon common = new ResultsDataCommon();
            SystemData systemData = Master.Instance.GetSystemData();
            common.API = systemData.API;
            common.RenderPipe = "Standard Legacy"; // TODO - Sort this
            common.SceneName = sceneName;
            common.TestName = testName;
            //Debug.LogWarning("BuildResultsDataCommon: " + common.DateTime + " - " + common.SceneName + " - " + common.TestName);
            return common;
        }
    }
}
