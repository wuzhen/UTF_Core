using UnityEngine;
using UnityEngine.UI;

namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // TestViewerNavbar
    // - Generates and manages tabs requested by TestDisplay

    public class TestViewerNavbar : MonoBehaviour 
	{
        // ------------------------------------------------------------------------------------
        // Variables

        // Singleton
        private static TestViewerNavbar _Instance = null;
        public static TestViewerNavbar Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = (TestViewerNavbar)FindObjectOfType(typeof(TestViewerNavbar));
                return _Instance;
            }
        }

        // Data
        public Text breadcrumbLabel;
		public RectTransform tabContentRect;
		public GameObject tabPrefab;
        int selectedTab;
        TestViewerTab[] tabs;

        // ------------------------------------------------------------------------------------
        // Generation

        // Set navbar context
        public void Generate(TestViewerTabData[] tabDatas)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Generating navbar"); // Write to console
            Cleanup(); // Cleanup
            GenerateBreadcrumb(); // Generate breadcrumb
            GenerateTabs(tabDatas); // Generate tabs
            tabs[0].EnableTab(); // Enable first tab
        }

        // Cleanup the navbar
        void Cleanup()
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Cleanup"); // Write to console
            if (tabs != null) // If tabs exist
            {
                foreach (TestViewerTab tab in tabs) // Iterate tabs
                    Destroy(tab.gameObject); // Delete them
                tabs = null; // Null the list
            }
        }

        // Set the breadcrumb label
        void GenerateBreadcrumb()
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Generating breadcrumb"); // Write to console
            TestEntry currentTest = TestRunner.Instance.GetCurrentTestEntry(); // Get the current test entry
            breadcrumbLabel.text = currentTest.suiteName + " - " + currentTest.typeName + " - " + currentTest.groupName + " - " + currentTest.testName; // Set breadcrumb label
        }

        // Generate new tabs
        void GenerateTabs(TestViewerTabData[] tabDatas)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Generating tabs"); // Write to console
            tabs = new TestViewerTab[tabDatas.Length]; // Create tab instances
            for (int i = 0; i < tabs.Length; i++) // Iterate
            {
                GameObject go = Instantiate(tabPrefab, tabContentRect, false); // Instantiate tab object
                TestViewerTab newTab = go.GetComponent<TestViewerTab>(); // Get TestViewerTab controller
                tabs[i] = newTab; // Set to array
                newTab.SetupTab(i, tabDatas[i]); // Setup
            }
        }        

        // ------------------------------------------------------------------------------------
        // Buttons

        // Show or hide the toolbar
        public void OnClickShowHide()
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Show/hide navbar"); // Write to console
            RectTransform rect = GetComponent<RectTransform>(); // Get rect
            bool isHidden = rect.anchoredPosition.y == 0 ? false : true; // Is the toolbar hidden
            float sizeY = isHidden ? 0 : -rect.sizeDelta.y; // Get new Y position value
            rect.anchoredPosition = new Vector2(0, sizeY); // Set position
            float statsPosition = isHidden ? 0 : 64; // Get offset for stats window
            TestViewerStatistics.Instance.SetPosition(statsPosition); // Set statistics window position
        }

        // When clicking a tab
		public void OnClickTab(int index)
		{
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Clicked: Navbar Tab"); // Write to console
            selectedTab = index; // Set selected index
			for(int i = 0; i < tabs.Length; i++) // Iterate tabs
					tabs[i].DisableTab(); // Disable them
            tabs[selectedTab].EnableTab(); // Enable selected
        }
	}
}
