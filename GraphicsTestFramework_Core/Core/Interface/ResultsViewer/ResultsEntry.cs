using UnityEngine;
using UnityEngine.UI;

namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // ResultsEntry
    // - Instance of an entry in the ResultsViewer

    public class ResultsEntry : MonoBehaviour
    {
        // ------------------------------------------------------------------------------------
        // Variables

        // UI Object References
        public Text groupNameText; 
        public Text testNameText;
        public Text passFailText;
        public Image passFailBackground;
        public Button expandButton;

        // Data
        string suiteName; // Track suite name to send to context object
        public ResultsIOData resultsData; // Results for this entry
        public TestLogicBase logic; // Logic script reference

        // ------------------------------------------------------------------------------------
        // Initialization

        // Setup the entry
        public void Setup(string suite, string groupName, string testName, ResultsIOData inputData, TestLogicBase inputLogic)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Setting up results entry"); // Write to console
            suiteName = suite; // Track suite name
            groupNameText.text = groupName; // Set group name label
            testNameText.text = testName; // Set test name label
            resultsData = inputData; // Set results data
            logic = inputLogic; // Track logic instance

            int passFail = 2; // Set default state (no results)
            if (resultsData != null) // If results data exists
                passFail = resultsData.resultsRow[0].resultsColumn[21] == "True" ? 1 : 0; // Set pass fail state

            switch (passFail) // Switch on pass fail
            {
                case 0: //Fail
                    passFailText.text = "FAIL"; // Set passfail label
                    passFailBackground.color = Menu.Instance.colors[4]; // Set passfail object color
                    expandButton.onClick.AddListener(delegate { ToggleContext(); }); // Add expand button listener
                    break;
                case 1: // Pass
                    passFailText.text = "PASS"; // Set passfail label
                    passFailBackground.color = Menu.Instance.colors[3]; // Set passfail object color
                    expandButton.onClick.AddListener(delegate { ToggleContext(); }); // Add expand button listener
                    break; 
                case 2: // No results
                    passFailText.text = "NONE"; // Set passfail label
                    passFailBackground.color = Menu.Instance.colors[1]; // Set passfail object color
                    expandButton.interactable = false; // Disable expand button
                    break;
            }
        }

        // ------------------------------------------------------------------------------------
        // Context Object

        // Toggle the context object
        public void ToggleContext()
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Toggle context object"); // Write to console
            logic.SetSuiteName(suiteName); // Set suite name on logic instance
            ResultsViewer.Instance.ToggleContextObject(this, logic.GetComponent<TestDisplayBase>()); // Tell ResultsViewer to toggle context object on this entry
        }
    }
}
