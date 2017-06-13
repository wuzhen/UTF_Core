using System;
using UnityEngine;
using UnityEngine.UI;

namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // Global Enums

    public enum TestViewerTabType { DefaultCamera, Camera, Texture, Material }

    // ------------------------------------------------------------------------------------
    // Global Data Structures

    [Serializable]
    public class TestViewerTabData
    {
        public string tabName; // Tab title
        public TestViewerTabType tabType; // Tab content type
        public object tabObject; // Tab content
        public TestViewerTabStatistic[] statistics; // Tab statistics list

        public TestViewerTabData(string name, TestViewerTabType type, object obj, TestViewerTabStatistic[] stats)
        {
            tabName = name;
            tabType = type;
            tabObject = obj;
            statistics = stats;
        }

        [Serializable]
        public class TestViewerTabStatistic
        {
            public string statLabel; // Statistic label
            public string statValue; // Statistic value

            public TestViewerTabStatistic(string label, string value)
            {
                statLabel = label;
                statValue = value;
            }
        }
    }

    // ------------------------------------------------------------------------------------
    // TestViewerTab
    // - Instance for a tab in the TestViewer

    public class TestViewerTab : MonoBehaviour 
	{
        // ------------------------------------------------------------------------------------
        // Variables

        TestViewerTabData tabData; // The tabs data
		public Text tabLabel; // Text field for title
		Button button; // Button for tab
		int tabIndex; // Index for tab

        // ------------------------------------------------------------------------------------
        // Initialization

        // Setup the tab
        public void SetupTab(int index, TestViewerTabData data)
		{
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Setting up tab"); // Write to console
            tabIndex = index; // Set tab index
			tabData = data; // Set tab data
			button = GetComponent<Button>(); // Get button
			button.onClick.AddListener(delegate { TestViewerNavbar.Instance.OnClickTab(tabIndex); }); // Add listener
			tabLabel.text = tabData.tabName; // Set tab label
		}

        // ------------------------------------------------------------------------------------
        // State

        // Enable the tab
        public void EnableTab()
		{
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Enabling tab"); // Write to console
            TestViewer.Instance.SetContext(tabData); // Set viewer context
            TestViewerStatistics.Instance.SetContext(tabData.statistics); // Set statistics window context
            button.interactable = false; // Disable the button
		}

        // Disable the tab
		public void DisableTab()
		{
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Disabling tab"); // Write to console
            button.interactable = true; // Enable the button
		}
    }
}
