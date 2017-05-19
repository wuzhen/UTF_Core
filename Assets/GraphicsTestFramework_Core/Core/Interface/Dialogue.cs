using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GraphicsTestFramework
{
	public class Dialogue : MonoBehaviour 
	{
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
		public DialogueDatabase dialogueDatabase;

		public GameObject dialogueObject;
		public Text messageText;
		public Text button0Label;
		public Button button0;
		public Text button1Label;
		public Button button1;
		public Text button2Label;
		public Button button2;
		public Toggle dontDisplayToggle;
		int activeDialogueId;

		public bool TryDialogue(bool active, int dialogueId, out Button[] buttons)
		{
			buttons = new Button[0];
			if(!dialogueDatabase.databaseEntries[dialogueId].ignoreDialogue)
			{
				buttons = SetState(active, dialogueId, buttons);
				return true;
			}
			else
			{
				return false;
			}
		}

		public void SetDontDisplay()
		{
			
		}

		public Button[] SetState(bool active, int dialogueId, Button[] buttons)
		{
			activeDialogueId = dialogueId;
			buttons = new Button[0];
			if(dialogueId >= dialogueDatabase.databaseEntries.Length)
			{
				Debug.LogWarning("Dialogue manager tried to access dialogue ID that is out of bounds. Aborting.");
			}
			else
			{
				dialogueObject.SetActive(active);
				if(active == true)
				{
					button0.gameObject.SetActive(false);
					button1.gameObject.SetActive(false);
					button2.gameObject.SetActive(false);
					messageText.text = dialogueDatabase.databaseEntries[dialogueId].message;
					if(dialogueDatabase.databaseEntries[dialogueId].hasDontDisplayToggle)
					{
						dontDisplayToggle.gameObject.SetActive(true);
					}
					else
					{
						dontDisplayToggle.gameObject.SetActive(false);
					}
					switch(dialogueDatabase.databaseEntries[dialogueId].buttons.Length)
					{
						case 1:
							button0.gameObject.SetActive(true);
							button0Label.text = dialogueDatabase.databaseEntries[dialogueId].buttons[0].buttonLabel;
							buttons = new Button[1];
							buttons[0] = button0;
							break;
						case 2:
							button1.gameObject.SetActive(true);
							button2.gameObject.SetActive(true);
							button1Label.text = dialogueDatabase.databaseEntries[dialogueId].buttons[0].buttonLabel;
							button2Label.text = dialogueDatabase.databaseEntries[dialogueId].buttons[1].buttonLabel;
							buttons = new Button[2];
							buttons[0] = button1;
							buttons[1] = button2;
							break;
					}
				}
				else
				{
					button0.onClick.RemoveAllListeners();
					button1.onClick.RemoveAllListeners();
					button2.onClick.RemoveAllListeners();
					if(dontDisplayToggle.isOn)
						dialogueDatabase.databaseEntries[activeDialogueId].ignoreDialogue = true;
					else
						dialogueDatabase.databaseEntries[activeDialogueId].ignoreDialogue = false;
					dontDisplayToggle.isOn = false;
					//dontDisplayToggle.gameObject.SetActive(false);
				}
			}
			return buttons;
		}

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
