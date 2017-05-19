using System;
using UnityEngine;
using UnityEngine.UI;

namespace GraphicsTestFramework
{
    public class MenuBreadcrumbEntry : MonoBehaviour
    {
        public MenuEntryData entryData;

        public Button mainButton;
        public Text entryLabel;

        /// ------------------------------------------------------------------------------------
        /// Setup MenuBreadcrumbEntry after being Instantiated

        public void Setup(MenuEntryData data, int state, int level)
        {
            switch (state)
            {
                case 0:     // New
                    entryData = Menu.Instance.CloneMenuEntryData(data);
                    mainButton.onClick.RemoveAllListeners();
                    mainButton.onClick.AddListener(delegate { Menu.Instance.OnBreadcrumbEntryClick(this); });
                    entryLabel.text = TestStructure.Instance.GetNameOfEntry(level, entryData.id.suiteId, entryData.id.typeId, entryData.id.sceneId, entryData.id.testId);
                    entryLabel.color = Menu.Instance.GetColor(0);
                    mainButton.interactable = true;
                    break;
                case 1:     // Clear
                    entryLabel.text = Menu.Instance.GetLevelName(level);
                    entryLabel.color = Menu.Instance.GetColor(1);
                    mainButton.interactable = false;
                    break;
                case 2:     // Ignore
                    break;
            }
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log("Menu set up breadcrumb entry for " + entryLabel.text);
        }

        /// ------------------------------------------------------------------------------------
        /// Setup MenuBreadcrumbEntry for Home button (happens once)
        
        public void SetupHome()
        {
            mainButton.onClick.RemoveAllListeners();
            mainButton.onClick.AddListener(delegate { Menu.Instance.OnBreadcrumbEntryClick(this); });
            mainButton.interactable = true;
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log("Menu set up breadcrumb entry for Home button");
        }
    }
}
