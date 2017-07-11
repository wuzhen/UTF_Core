using System;
using UnityEngine;
using UnityEngine.UI;

namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // TestViewerToolbar
    // - Controls test run and navigation within the viewer

    public class TestViewerToolbar : MonoBehaviour 
	{
        // ------------------------------------------------------------------------------------
        // Variables

        // Singleton
        private static TestViewerToolbar _Instance = null;
        public static TestViewerToolbar Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = (TestViewerToolbar)FindObjectOfType(typeof(TestViewerToolbar));
                return _Instance;
            }
        }

        // Buttons
        public TestViewerToolbarButtons buttons;
        [Serializable]
        public class TestViewerToolbarButtons
        {
            public Button showHideButton;
            public Button previousButton;
            public Button nextButton;
            public Button saveResultsButton;
            public Button saveBaselineButton;
            public Button restartTestButton;
            public Button returnToMenuButton;
            public Button toggleStatisticsButton;
        }

        // ------------------------------------------------------------------------------------
        // Context & State

        // Set buttons based on baseline resolution mode
        public void SetContext(State input)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Setting context"); // Write to console
            buttons.previousButton.interactable = input.previous; // Set interactable
            buttons.nextButton.interactable = input.next; // Set interactable
            buttons.saveResultsButton.interactable = input.results; // Set interactable
            buttons.saveBaselineButton.interactable = input.baseline; // Set interactable
            buttons.restartTestButton.interactable = input.restart; // Set interactable
        }

        // State set
        public class State
        {
            public bool previous;
            public bool next;
            public bool results;
            public bool baseline;
            public bool restart;

            public State(bool prv, bool nxt, bool rlt, bool bsl, bool rst)
            {
                previous = prv;
                next = nxt;
                results = rlt;
                baseline = bsl;
                restart = rst;
            }
        }

        // ------------------------------------------------------------------------------------
        // Buttons

        // Show or hide the toolbar
        public void OnClickShowHide()
		{
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Show/hide toolbar"); // Write to console
            RectTransform rect = GetComponent<RectTransform>(); // Get rect
            bool isHidden = rect.anchoredPosition.y == 0 ? true : false; // Is the toolbar hidden
            float sizeY = isHidden ? rect.sizeDelta.y : 0; // Get new Y position value
            rect.anchoredPosition = new Vector2(0, sizeY); // Set position
		}

        // On click previous button
        public void OnClickPrevious()
		{
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Clicked: Previous"); // Write to console
            TestRunner.Instance.EndTest(); // Need to disable test objects TODO - Consider using broadcast here
			TestRunner.Instance.PreviousTest(); // Move to previous
		}

        // On click next button
        public void OnClickNext()
		{
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Clicked: Next"); // Write to console
            if (!TestRunner.Instance.CheckEndOfRunner()) // Check for end of runner
            {
                TestRunner.Instance.EndTest(); // End the current test
                if(TestRunner.Instance.runnerType != RunnerType.Automation) // If not automation (handled automatically)
                    TestRunner.Instance.NextTest(); // Move to next test
            }
            else
                OnClickReturnToMenu(); // Return to menu
		}

        // On click save results button
        public void OnClickSaveResults()
		{
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Clicked: Save Results"); // Write to console
            TestTypeManager.Instance.GetActiveTestLogic().SubmitResults(0); // Send results data
		}

        // On click save baseline button
        // TODO - Refactor
        public void OnClickSaveBaseline()
		{
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Clicked: Save Baseline"); // Write to console
            Button[] buttons = new Button[2]; // Create button array
			bool openDialogue = Dialogue.Instance.TryDialogue(true, 0, out buttons); // Try for dialogue window and out buttons
			if(openDialogue) // If dialogue opens
			{
				buttons[0].onClick.AddListener(delegate { SaveBaselineAction(); }); // Add listeners
				buttons[0].onClick.AddListener(delegate { Dialogue.Instance.SetState(false, 0); });
				buttons[1].onClick.AddListener(delegate { Dialogue.Instance.SetState(false, 0); });
			}
			else
				SaveBaselineAction(); // Save baseline
		}

		// Save baseline action
		void SaveBaselineAction()
		{
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Save baseline action"); // Write to console
            TestTypeManager.Instance.GetActiveTestLogic().SubmitResults(1); // Send baseline data
		}

        // On click restart test button
        public void OnClickRestartTest()
		{
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Clicked: Restart Test"); // Write to console
            TestTypeManager.Instance.GetActiveTestLogic().RestartTest(); // Restart the test
		}

        // On click return to menu button
        public void OnClickReturnToMenu()
		{
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Clicked: Return to Menu"); // Write to console
            TestRunner.Instance.EndTest(); // End the test
			TestViewer.Instance.SetState(false); // Disable the TestViewer
			Menu.Instance.SetMenuState(true); // Enable the Menu
		}

        // On click toggle statistics button
        public void OnClickToggleStatistics()
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Clicked: Toggle Statistics"); // Write to console
            TestViewerStatistics.Instance.ToggleVisible(); // Toggle the window
        }
    }
}
