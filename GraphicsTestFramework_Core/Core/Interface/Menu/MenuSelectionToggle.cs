using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // MenuSelectionToggle
    // - Handles toggle selection for MenuListEntry

    public class MenuSelectionToggle : MonoBehaviour
    {
        // ------------------------------------------------------------------------------------
        // Variables

        public Button toggleButton;
        public int state;
        public Image stateImage;
        public Sprite[] stateSprites;
        MenuListEntry owner;

        // ------------------------------------------------------------------------------------
        // Initialization

        // Set up the selection toggle
        public void Setup(MenuListEntry input)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Setting up selection toggle"); // Write to console
            owner = input; // Set owner as list entry
            toggleButton.onClick.RemoveAllListeners(); // Remove listeners
            toggleButton.onClick.AddListener(delegate { OnButtonClick(); }); // Add listener
        }

        // ------------------------------------------------------------------------------------
        // Selection

        // On toggle
        public void OnButtonClick()
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Clicked: Selection Toggle"); // Write to console
            switch (state)
            {
                case 0:     // Unselected
                    state = 1;
                    break;
                case 1:     // Selected
                    state = 0;
                    break;
                case 2:     // Some selected children
                    state = 0;
                    break;
            }
            stateImage.sprite = stateSprites[state]; // Switch sprite
            owner.ChangeSelection(state); // Change selection on owner
        }

        // Selection state set by menu entry
        public void SetSelectionState(int input)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Setting selection state"); // Write to console
            state = input; // Set state
            stateImage.sprite = stateSprites[input]; // Switch sprite
        }
    }
}
