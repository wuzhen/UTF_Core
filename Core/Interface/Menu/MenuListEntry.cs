using UnityEngine;
using UnityEngine.UI;

namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // MenuListEntry
    // - Instance of an entry in main list

    public class MenuListEntry : MonoBehaviour
    {
        // ------------------------------------------------------------------------------------
        // Variables

        public MenuEntryData entryData;
        public Button mainButton;
        public MenuSelectionToggle selectionToggle;
        public Text entryLabel;

        // ------------------------------------------------------------------------------------
        // Initialization

        // Setup the entry
        public void Setup(MenuEntryData data)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Setting up menu list entry"); // Write to console
            entryData = Menu.Instance.CloneMenuEntryData(data); // Clone the entry data
            entryLabel.text = entryData.entryName; // Set label text
            selectionToggle.Setup(this); // Setup the selection toggle
            selectionToggle.SetSelectionState(entryData.selectionState); // Set selection state of the toggle
            mainButton.onClick.RemoveAllListeners(); // Remove button listeners
            mainButton.onClick.AddListener(delegate { Menu.Instance.OnListEntryClick(this); }); // Add listener
        }

        // ------------------------------------------------------------------------------------
        // Selection

        // Change selection state for this entry
        public void ChangeSelection(int state)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Changing selection state for entry " + entryData.entryName); // Write to console
            entryData.selectionState = state; // Set selection state on this entry
            TestStructure.Instance.SetSelectionState(entryData); // Set selection state on TestStructure
            Menu.Instance.CheckRunButtonStatus(); // Check run status on Menu
        }
    }

    // ------------------------------------------------------------------------------------
    // Global Data Structures

    [System.Serializable]
    public class MenuEntryData
    {
        public string entryName;
        public int selectionState;
        public MenuTestEntry id;
    }
}
