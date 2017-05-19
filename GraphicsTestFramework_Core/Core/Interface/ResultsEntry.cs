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

        public void Setup(string sceneName, string testName, int passFail)
        {
            sceneNameText.text = sceneName;
            testNameText.text = testName;
            switch(passFail)
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
    }
}
