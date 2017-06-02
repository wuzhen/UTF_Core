using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GraphicsTestFramework
{
	public class TestViewer : MonoBehaviour 
	{
        // TODO - Do a proper job of test viewer models based on test logic type
        // - Variable UI, instantiate various bars top and bottom based on custom viewer models
        // - Consider removing distinction between tool/nav bars

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

        public ViewerToolbar viewerToolbar;
        public ViewerNavigationBar viewerNavigationBar;
        public RawImage textureImage; // USed to display tetxures in the viewer
        Camera currentCamera; //Used to track the camera current set to high depth (to set back after)
        float cameraDepth; //Used to track tahe above cameras depth

		public void SetTestViewerState(int state, ViewerType type, object contextObject)
        {
            switch(state)
            {
                case 0: // off
                    this.gameObject.SetActive(false);
                    break;
                case 1:
                    this.gameObject.SetActive(true);
                    UpdateViewerContext(type, contextObject);
                    break;
            }
        }
        
        // Update the viewer context based on the active TestRunner RunnerType
        void UpdateViewerContext(ViewerType type, object contextObject)
        {
            switch(TestRunner.Instance.runnerType)
            {
                case RunnerType.Automation: // TODO - This mode should never be passed to the Viewer. Remove when sure.
                    Debug.LogWarning("Run runner type was passed to the TestViewer. This shouldnt happen!");
                    break;
                case RunnerType.Manual:
                    viewerToolbar.SetDefaultMode();
                    viewerNavigationBar.UpdateNavigationBar(type, contextObject);
                    // TODO - Set up full test viewer context here
                    break;
                case RunnerType.Resolve:
                    viewerToolbar.SetBaselineResolutionMode();
                    viewerNavigationBar.UpdateNavigationBar(type, contextObject);
                    break;
            }
        }

        // Sets the viewers content TODO - This is getting messy, revisit all of this...
        public void SetViewerContent(ViewerBarTabType type, object content)
        {
            switch(type)
			{
				case ViewerBarTabType.Camera:
                    if(currentCamera)
                        currentCamera.depth = cameraDepth;
                    currentCamera = (Camera)content;
                    currentCamera.depth = 9;
                    cameraDepth = currentCamera.depth;
                    textureImage.gameObject.SetActive(false);
                    textureImage.material = null;
                    break;
				case ViewerBarTabType.Texture:
                    textureImage.texture = (Texture2D)content;
                    textureImage.gameObject.SetActive(true);
                    textureImage.material = null;
                    break;
                case ViewerBarTabType.Material:
                    //textureImage.texture = null;
                    textureImage.material = (Material)content;
                    textureImage.gameObject.SetActive(true);
                    break;
            }
        }        
	}

    public enum ViewerType { Default, DefaultTabs }

    public enum ViewerBarTabType { Camera, Texture, Material }

    [System.Serializable]
    public class ViewerBarTabData
    {
        public string tabName;
        public ViewerBarTabType tabType;
        public object tabObject;
        public Vector2 textureResolution;
    }
}
