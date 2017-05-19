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

        public GameObject tabPrefab;
        public GameObject resultsEntryPrefab;

        public GameObject resultsParent;
        public RectTransform suiteTabContentRect;
        public RectTransform typeTabContentRect;
        public RectTransform listContentRect;

        int selectedSuite;
        int selectedType;
        List<GameObject> suiteTabs = new List<GameObject>();
        List<GameObject> typeTabs = new List<GameObject>();
        List<GameObject> listEntries = new List<GameObject>();
        float entryHeight;

        public void Enable()
        {
            resultsParent.SetActive(true);
            GenerateSuiteTabs();
        }

        public void BackToMenu()
        {
            Cleanup();
            resultsParent.SetActive(false);
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
            int sceneCount = TestStructure.Instance.testStructure.suites[selectedSuite].types[selectedType].scenes.Count;
            for (int sc = 0; sc < sceneCount; sc++)
            {
                string sceneName = TestStructure.Instance.testStructure.suites[selectedSuite].types[selectedType].scenes[sc].sceneName;
                int testCount = TestStructure.Instance.testStructure.suites[selectedSuite].types[selectedType].scenes[sc].tests.Count;
                for (int te = 0; te < testCount; te++)
                {
                    string testName = TestStructure.Instance.testStructure.suites[selectedSuite].types[selectedType].scenes[sc].tests[te].testName;
                    ResultsDataCommon common = BuildResultsDataCommon(sceneName, testName);
                    ResultsIOData data = ResultsIO.Instance.RetrieveResult(TestStructure.Instance.testStructure.suites[selectedSuite].suiteName, TestStructure.Instance.testStructure.suites[selectedSuite].types[selectedType].typeName, common);

                    int passFail = 2;
                    if (data != null)
                        passFail = data.resultsRow[0].resultsColumn[17] == "True" ? 1 : 0; // TODO - Cast this back to correct results

                    GameObject go = Instantiate(resultsEntryPrefab, listContentRect, false);
                    listEntries.Add(go);
                    RectTransform goRect = go.GetComponent<RectTransform>();
                    goRect.anchoredPosition = new Vector2(0, entryHeight);
                    ResultsEntry newEntry = go.GetComponent<ResultsEntry>();
                    newEntry.Setup(common.SceneName, common.TestName, passFail);
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

        ResultsDataCommon BuildResultsDataCommon(string sceneName, string testName)
        {
            ResultsDataCommon common = new ResultsDataCommon();
            SystemData systemData = Master.Instance.GetSystemData();
            common.API = systemData.API;
            common.RenderPipe = "Standard Legacy"; // TODO - Sort this
            common.SceneName = sceneName;
            common.TestName = testName;
            return common;
        }
    }
}
