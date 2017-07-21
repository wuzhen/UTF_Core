using UnityEngine;
using UnityEngine.UI;

namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // MenuBreadcrumbEntry
    // - Instance of an entry in main breadcrumb

    public class MenuBreadcrumbEntry : MonoBehaviour
    {
        // ------------------------------------------------------------------------------------
        // Variables

        public MenuEntryData entryData;
        public Button mainButton;
        public Text entryLabel;

        // ------------------------------------------------------------------------------------
        // Initialization

        // Setup the breacrumb
        public void Setup(MenuEntryData data, int state, int level)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Setting up menu breadcrumb entry"); // Write to console
            switch (state)
            {
                case 0:     // New
                    entryData = Menu.Instance.CloneMenuEntryData(data); // Clone the entry data
                    mainButton.onClick.RemoveAllListeners(); // Remove listeners
                    mainButton.onClick.AddListener(delegate { Menu.Instance.OnBreadcrumbEntryClick(this); }); // Add listener
                    entryLabel.text = TestStructure.Instance.GetNameOfEntry(level, entryData.id.suiteId, entryData.id.typeId, entryData.id.groupId, entryData.id.testId); // Get the label text
                    entryLabel.color = Menu.Instance.GetColor(0); // Set color
                    mainButton.interactable = true; // Set interactable
                    break;
                case 1:     // Clear
                    entryLabel.text = Menu.Instance.GetLevelName(level); // Get level name and set to label
                    entryLabel.color = Menu.Instance.GetColor(1); // Set color
                    mainButton.interactable = false; // Set non-interactable
                    break;
                case 2:     // Ignore
                    break;
            }
        }

        // Setup the home button
        public void SetupHome()
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Setting up home breadcrumb entry"); // Write to console
            mainButton.onClick.RemoveAllListeners(); // Clear listeners
            mainButton.onClick.AddListener(delegate { Menu.Instance.OnBreadcrumbEntryClick(this); }); // Add listener
            mainButton.interactable = true; // Set interactable
        }
    }
}
