using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GraphicsTestFramework
{
    public class ResultsEntry : MonoBehaviour
    {
        public Text sceneNameText;
        public Text testNameText;
        public Text passFailText;
        public Image passFailBackground;
        public Button expandButton;

        ResultsIOData resultsData;
        TestLogicBase logic;

        public void Setup(string sceneName, string testName, ResultsIOData inputData, TestLogicBase logic)
        {
            sceneNameText.text = sceneName;
            testNameText.text = testName;
            resultsData = inputData;

            int passFail = 2;
            if (resultsData != null)
                passFail = resultsData.resultsRow[0].resultsColumn[17] == "True" ? 1 : 0; // TODO - Cast this back to correct results?

            switch (passFail)
            {
                case 0: //Fail
                    passFailText.text = "FAIL";
                    passFailBackground.color = Menu.Instance.colors[4];
                    break;
                case 1: // Pass
                    passFailText.text = "PASS";
                    passFailBackground.color = Menu.Instance.colors[3];
                    break; 
                case 2: // No results
                    passFailText.text = "N/A";
                    passFailBackground.color = Menu.Instance.colors[1];
                    break;
            }
        }

        public void ExpandContext()
        {
            ResultsViewer.Instance.ShowContextObject(this, logic.GetComponent<TestDisplayBase>());
        }

        public void HideContext()
        {
            ResultsViewer.Instance.HideContextObject(this);
        }
    }
}
