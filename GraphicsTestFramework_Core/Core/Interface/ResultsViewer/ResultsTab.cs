using UnityEngine;
using UnityEngine.UI;

namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // ResultsTab
    // - Instance of a tab in the ResultsViewer

    public class ResultsTab : MonoBehaviour
    {
        // ------------------------------------------------------------------------------------
        // Local Enums

        public enum TabType { Suite, TestType }

        // ------------------------------------------------------------------------------------
        // Variables

        public TabType tabType;
        public int index;
        public Text label;

        // ------------------------------------------------------------------------------------
        // Initialization


        // Setup the tab
        public void Setup(TabType type, int id, string title)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Setting up results tab"); // Write to console
            tabType = type; // Set tab type
            index = id; // Set tab index
            label.text = title; // Set tab label 
            switch(tabType) // Switch on tab type
            {
                case TabType.Suite:     // Suite
                    GetComponent<Button>().onClick.AddListener(delegate { ResultsViewer.Instance.SelectSuite(index); }); // Add listener
                    break;
                case TabType.TestType:  // Type
                    GetComponent<Button>().onClick.AddListener(delegate { ResultsViewer.Instance.SelectType(index); }); // Add listener
                    break;
            }
        }
    }
}
