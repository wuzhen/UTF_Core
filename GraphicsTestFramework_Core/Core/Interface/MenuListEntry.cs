using System;
using UnityEngine;
using UnityEngine.UI;

namespace GraphicsTestFramework
{
    public class MenuListEntry : MonoBehaviour
    {
        public MenuEntryData entryData;

        public Button mainButton;
        public SelectionToggle selectionToggle;
        public Text entryLabel;

        /// ------------------------------------------------------------------------------------
        /// Setup MenuListEntry after being Instantiated

        public void Setup(MenuEntryData data)
        {
            entryData = Menu.Instance.CloneMenuEntryData(data);
            //entryData.id.currentLevel++;
            entryLabel.text = entryData.entryName;
            selectionToggle.Setup(this);
            selectionToggle.SetSelectionState(entryData.selectionState);
            mainButton.onClick.RemoveAllListeners();
            mainButton.onClick.AddListener(delegate { Menu.Instance.OnListEntryClick(this); });
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log("Menu set up entry for " + entryData.entryName);
        }

        /// ------------------------------------------------------------------------------------
        /// Change selection state (Called by SelectionToggle)
        
        public void ChangeSelection(int state)
        {
            entryData.selectionState = state;
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log("Menu changed selection state for entry " + entryData.entryName);
            TestStructure.Instance.SetSelectionState(entryData);
            Menu.Instance.CheckRunButtonStatus();
        }
    }

    /// ------------------------------------------------------------------------------------
    /// Public Data Structures

    [System.Serializable]
    public class MenuEntryData
    {
        public string entryName;
        public int selectionState;
        public MenuTestEntry id;
    }
}
