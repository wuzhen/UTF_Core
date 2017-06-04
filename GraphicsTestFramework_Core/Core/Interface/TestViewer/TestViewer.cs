using UnityEngine;
using UnityEngine.UI;

namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // TestViewer
    // - The main TestViewer controller
    // - Controls main viewer context
    // - Higher level control of Tool and Nav bars

    public class TestViewer : MonoBehaviour 
	{
        // ------------------------------------------------------------------------------------
        // Variables

        // Singleton
        private static TestViewer _Instance = null;
        public static TestViewer Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = (TestViewer)FindObjectOfType(typeof(TestViewer));
                return _Instance;
            }
        }

        // Data
        public GameObject testViewerParent; // Object to enable/disable
        public RawImage textureImage; // Viewer texture display
        Camera currentCamera; // Tacking active high depth camera
        float cameraDepth; // Tracking cameras previous depth

        // ------------------------------------------------------------------------------------
        // Content & State

        // Enable or disable the viewer
        public void SetState(bool state)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Setting state to "+state); // Write to console
            testViewerParent.SetActive(state); // Set state
        }
        
        // Update the viewer tool bar and nav bar context based on the active TestRunner RunnerType
        public void UpdateBars(TestViewerTabData[] tabDatas)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Updating bars"); // Write to console
            TestViewerToolbar.Instance.SetContext(TestRunner.Instance.runnerType == RunnerType.Resolve ? true : false); // Set resolution mode
            TestViewerNavbar.Instance.Generate(tabDatas); // Update navigation bar
        }

        // Sets the viewers content
        public void SetContext(TestViewerTabData tabData)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Settings context for type "+tabData.tabType); // Write to console
            switch (tabData.tabType) // Switch based on tab type
			{
				case TestViewerTabType.Camera:
                    SetCameraValues(tabData); // Set camera values
                    textureImage.gameObject.SetActive(false); // Disable image
                    textureImage.material = null; // Null material
                    break;
				case TestViewerTabType.Texture:
                    textureImage.texture = (Texture2D)tabData.tabObject; // Set image texture
                    textureImage.gameObject.SetActive(true); // Enable image
                    textureImage.material = null; // Null material
                    break;
                case TestViewerTabType.Material:
                    textureImage.material = (Material)tabData.tabObject; // Set image material
                    textureImage.gameObject.SetActive(true); // Enable image
                    break;
            }
        }     
        
        // Set camera values
        void SetCameraValues(TestViewerTabData tabData)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Setting camera values"); // Write to console
            if (currentCamera) // If a current camera exists
                currentCamera.depth = cameraDepth; // Reset its depth
            currentCamera = (Camera)tabData.tabObject; // Get new current camera
            cameraDepth = currentCamera.depth; // Get current depth
            currentCamera.depth = 9; // Set its depth
        }
    }
}
