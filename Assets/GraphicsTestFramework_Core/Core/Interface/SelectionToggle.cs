using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GraphicsTestFramework
{
    public class SelectionToggle : MonoBehaviour
    {
        public Button toggleButton;
        public int state;
        public Image stateImage;
        public Sprite[] stateSprites;
        MenuListEntry owner;

        public void Setup(MenuListEntry input)
        {
            owner = input;
            toggleButton.onClick.RemoveAllListeners();
            toggleButton.onClick.AddListener(delegate { OnButtonClick(); });
        }

        public void OnButtonClick()
        {
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
            stateImage.sprite = stateSprites[state];
            owner.ChangeSelection(state);
        }

        public void SetSelectionState(int input)
        {
            state = input;
            stateImage.sprite = stateSprites[input];
        }
    }
}
