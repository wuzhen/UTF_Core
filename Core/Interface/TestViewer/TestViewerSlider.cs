using UnityEngine;
using UnityEngine.UI;

namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // TestViewerSlider
    // - Controls context for the TextureSlider and MaterialSlider tab types
    // - Controls updating slider and mask positioning for the display type

    public class TestViewerSlider : MonoBehaviour
    {
        // ------------------------------------------------------------------------------------
        // Variables

        // UI References
        public GameObject container;
        public Slider slider;
        public Image maskImage;
        public RawImage image1;
        public RawImage image2;
        public Text label1;
        public Text label2;

        // Data
        bool isActive = false;
        float previousSliderValue = 0;

        // ------------------------------------------------------------------------------------
        // State & Context

        // Set the active state
        public void SetState(bool state)
        {
            isActive = state; // Track active state
            container.SetActive(isActive); // Set active to requested
        }

        // Set context for TextureSlider
        public void SetContext(TestViewer.TextureSliderContext input)
        {
            image1.material = null; // Null material
            image2.material = null; // Null material
            image1.texture = input.image1; // Set texture 1
            image2.texture = input.image2; // Set texture 2
            label1.text = input.label1; // Set label 1
            label2.text = input.label2; // Set label 2
        }

        // Set context for MaterialSlider
        public void SetContext(TestViewer.MaterialSliderContext input)
        {
            image1.material = input.image1; // Set material 1
            image2.material = input.image2; // Set material 2
            image1.texture = image1.material.GetTexture("_MainTex"); // Set texture for material 1
            image2.texture = image2.material.GetTexture("_MainTex"); // Set texture for material 2
            label1.text = input.label1; // Set label 1
            label2.text = input.label2; // Set label 2
        }
        
        // Every frame
        void Update()
        {
            if (isActive) // If active
            {
                if(slider.value != previousSliderValue) // If slider value has changed
                {
                    maskImage.fillAmount = slider.value; // Set mask to slider value
                    previousSliderValue = slider.value; // Track slider value
                }
            }    
        }
    }
}

