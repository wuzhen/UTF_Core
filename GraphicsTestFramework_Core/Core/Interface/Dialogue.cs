using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // Dialogue
    // - Handles dialogue windows

    public class Dialogue : MonoBehaviour 
	{
        // ------------------------------------------------------------------------------------
        // Variables

        // Singleton
        private static Dialogue _Instance = null;
        public static Dialogue Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = (Dialogue)FindObjectOfType(typeof(Dialogue));
                return _Instance;
            }
        }

        // Data
		public DialogueDatabase dialogueDatabase;

        // References
        public GameObject dialogueObject;
		public Text messageText;
		public Text button0Label;
		public Button button0;
		public Text button1Label;
		public Button button1;
		public Text button2Label;
		public Button button2;
		public Toggle dontDisplayToggle;

        // ------------------------------------------------------------------------------------
        // Request Dialogue

        // Attempt to open a dialogue window for a specific ID
        public bool TryDialogue(bool active, int dialogueId, out Button[] buttons)
		{
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Trying for dialogue for ID "+dialogueId); // Write to console
            buttons = new Button[0]; // Create buttons to return
			if(!dialogueDatabase.databaseEntries[dialogueId].ignoreDialogue) // If not set to ignore
			{
				buttons = SetState(active, dialogueId); // Set dialogue state and return buttons to set from caller
				return true; // Dialogue created
			}
			else
				return false; // Dialogue not created
        }

        // Set dialogue window state from ID and return buttons to be set from caller
		public Button[] SetState(bool active, int dialogueId)
		{
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Setting dialogue state to "+active); // Write to console
            //activeDialogueId = dialogueId;
            Button[] buttons = new Button[0]; // Create buttons to return
			if(dialogueId >= dialogueDatabase.databaseEntries.Length) // If invalid
                Console.Instance.Write(DebugLevel.Critical, MessageLevel.Log, "Dialogue manager tried to access dialogue ID that is out of bounds. Aborting."); // Write to console
			else // Valid
			{
				dialogueObject.SetActive(active); // Enable/disable dialogie screen
				if(active == true) // If enabling
				{
					button0.gameObject.SetActive(false); // Disable all buttons
                    button1.gameObject.SetActive(false);
					button2.gameObject.SetActive(false);
					messageText.text = dialogueDatabase.databaseEntries[dialogueId].message; // Set message text
					if(dialogueDatabase.databaseEntries[dialogueId].hasDontDisplayToggle) // If the entry has a dont display toggle
						dontDisplayToggle.gameObject.SetActive(true); // Enable it
					else
						dontDisplayToggle.gameObject.SetActive(false); // Disable it
					switch(dialogueDatabase.databaseEntries[dialogueId].buttons.Length) // Switch on button count
					{
						case 1:
							button0.gameObject.SetActive(true); // Enable center button
							button0Label.text = dialogueDatabase.databaseEntries[dialogueId].buttons[0].buttonLabel; // Set button label
							buttons = new Button[1]; // Create button to return
							buttons[0] = button0; // Assign
							break;
						case 2:
							button1.gameObject.SetActive(true); // Enable dual buttons
							button2.gameObject.SetActive(true);
							button1Label.text = dialogueDatabase.databaseEntries[dialogueId].buttons[0].buttonLabel; // Set button labels
							button2Label.text = dialogueDatabase.databaseEntries[dialogueId].buttons[1].buttonLabel;
							buttons = new Button[2]; // Create button array to return
							buttons[0] = button1; // Assign
							buttons[1] = button2;
							break;
					}
				}
				else // If disabling
				{
					button0.onClick.RemoveAllListeners(); // Remove button listeners
					button1.onClick.RemoveAllListeners();
					button2.onClick.RemoveAllListeners();
					if(dontDisplayToggle.isOn) // If dont display set
						dialogueDatabase.databaseEntries[dialogueId].ignoreDialogue = true; // Ignore in future
					else
						dialogueDatabase.databaseEntries[dialogueId].ignoreDialogue = false; // Dont ignore
					dontDisplayToggle.isOn = false; // Reset
				}
			}
			return buttons; // Return
		}

        // ------------------------------------------------------------------------------------
        // Local Reference Structures

        [Serializable]
		public class DialogueDatabase
		{
			public DialogueEntry[] databaseEntries;
		}

		[Serializable]
		public class DialogueEntry
		{
			public string message;
			public DialogueButton[] buttons;
			public bool hasDontDisplayToggle;
			public bool ignoreDialogue;
		}

		[Serializable]
		public class DialogueButton
		{
			public string buttonLabel;
		}
	}
}
