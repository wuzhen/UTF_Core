using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // MenuSettings
    // - Controller for the settings menu

    public class MenuSettings : MonoBehaviour
    {
        // ------------------------------------------------------------------------------------
        // Variables

        // Singleton
        private static MenuSettings _Instance = null;
        public static MenuSettings Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = (MenuSettings)FindObjectOfType(typeof(MenuSettings));
                return _Instance;
            }
        }

        // UI Object References
        public GameObject menuSettingsParent;

        // ------------------------------------------------------------------------------------
        // Content & State

        // Enable or disable the settings menu
        public void SetState(bool state)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Setting state to " + state); // Write to console
            menuSettingsParent.SetActive(state); // Set state
        }

        // ------------------------------------------------------------------------------------
        // Buttons

        // On button click: Clear local data
        public void OnClickClearLocalData()
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Clicked: Clear Local Data"); // Write to console
            Button[] buttons = new Button[2]; // Create button array
            bool openDialogue = Dialogue.Instance.TryDialogue(true, 1, out buttons); // Try for dialogue window and out buttons
            if (openDialogue) // If dialogue opens
            {
                buttons[0].onClick.AddListener(delegate { ClearLocalDataAction(); }); // Add listeners
                //buttons[0].onClick.AddListener(delegate { Dialogue.Instance.SetState(false, 1); });
                buttons[1].onClick.AddListener(delegate { Dialogue.Instance.SetState(false, 1); });
            }
            else
                ClearLocalDataAction(); // Save baseline
            
        }

        // Clear all local data
        void ClearLocalDataAction()
        {
            LocalIO.Instance.ClearLocalData(); // Tell IO to clear all local data
        }
    }
}
