using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GraphicsTestFramework
{
	public class ViewerToolbar : MonoBehaviour 
	{
		public Button showHideButton;
		public Button previousButton;
		public Button nextButton;
		public Button saveResultsButton;
		public Button saveBaselineButton;
		public Button restartTestButton;
		public Button returnToMenuButton;

		bool isHidden = true;
		RectTransform thisRect;
		bool isBaselineResolution;

		void Start()
		{
			thisRect = GetComponent<RectTransform>();
		}

		public void OnClickShowHide()
		{
			isHidden = !isHidden;
			switch(isHidden)
			{
				case false:
					thisRect.anchoredPosition = new Vector2(0, thisRect.sizeDelta.y);
					break;
				case true:
					thisRect.anchoredPosition = new Vector2(0,0);
					break;
			}
		}

		public void SetDefaultMode()
		{
			previousButton.interactable = true;
			nextButton.interactable = true;
			saveResultsButton.interactable = true;
			saveBaselineButton.interactable = true;
			restartTestButton.interactable = true;
			returnToMenuButton.interactable = true;
			isBaselineResolution = false;
		}

		public void SetBaselineResolutionMode()
		{
			previousButton.interactable = false;
			nextButton.interactable = false;
			saveResultsButton.interactable = false;
			saveBaselineButton.interactable = true;
			restartTestButton.interactable = true;
			returnToMenuButton.interactable = true;
			isBaselineResolution = true;
		}

		public void OnClickPrevious()
		{
			TestList.Instance.activeTestLogic.Cleanup(); //Need to cleanup test logic // TODO - can do this cleaner?
			TestList.Instance.EndTest(); // Need to disable test objects TODO - Consider using broadcast here
			TestRunner.Instance.PreviousTest(); // Move to previous
		}

		public void OnClickNext()
		{
			if (!TestRunner.Instance.CheckEndOfRunner())
            {
                TestList.Instance.activeTestLogic.Cleanup(); //Need to cleanup test logic // TODO - can do this cleaner?
                TestList.Instance.EndTest(); // Need to disable test objects TODO - Consider using broadcast here
                TestRunner.Instance.NextTest(); // Move to next
            }
            else
            {
                OnClickReturnToMenu();
            }
		}

		public void OnClickSaveResults()
		{
			TestList.Instance.activeTestLogic.SendDataToResultsIO(0); //Send results data // TODO - can do this cleaner?
		}

		public void OnClickSaveBaseline()
		{
			UnityEngine.UI.Button[] buttons = new UnityEngine.UI.Button[2]; 
			bool openDialogue = Dialogue.Instance.TryDialogue(true, 0, out buttons);
			if(openDialogue)
			{
				buttons[0].onClick.AddListener(delegate { SaveBaselineAction(); });
				buttons[0].onClick.AddListener(delegate { Dialogue.Instance.SetState(false, 0, null); });
				buttons[1].onClick.AddListener(delegate { Dialogue.Instance.SetState(false, 0, null); });
			}
			else
			{
				SaveBaselineAction();
			}
		}

		//Have to separate this so it can be called depending on the dialogue box
		void SaveBaselineAction()
		{
			TestList.Instance.activeTestLogic.SendDataToResultsIO(1); //Send baseline data // TODO - can do this cleaner?
			if(isBaselineResolution)
				OnClickNext();
		}

		public void OnClickRestartTest()
		{
			TestList.Instance.activeTestLogic.RestartTest(); //Restart the test // TODO - can do this cleaner?
		}

		public void OnClickReturnToMenu()
		{
			TestList.Instance.activeTestLogic.Cleanup(); //Need to cleanup test logic // TODO - can do this cleaner?
			TestList.Instance.EndTest(); // Need to disable test objects TODO - Consider using broadcast here
			TestViewer.Instance.SetTestViewerState(0, ViewerType.Default, null);
			Menu.Instance.SetMenuState(1);
		}
	}
}
