using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GraphicsTestFramework
{
	public class ViewerBarTab : MonoBehaviour 
	{
		ViewerBarTabData tabData;
		public Text tabLabel;
		public Texture2D tabTexture;
        public Material tabMaterial;
		Button button;
		int tabIndex;

		public void SetupTab(int index, ViewerBarTabData data)
		{
			tabIndex = index;
			tabData = data;
			button = GetComponent<Button>();
			button.onClick.AddListener(delegate { ViewerNavigationBar.Instance.OnClickTab(tabIndex); });
			tabLabel.text = tabData.tabName;
		}

		public void EnableTab()
		{
			switch(tabData.tabType)
			{
				case ViewerBarTabType.Camera:
                    TestViewer.Instance.SetViewerContent(tabData.tabType, tabData.tabObject);
					break;
				case ViewerBarTabType.Texture:
                    tabTexture = (Texture2D)tabData.tabObject;
					TestViewer.Instance.SetViewerContent(tabData.tabType, tabTexture);
					break;
                case ViewerBarTabType.Material:
                    tabMaterial = (Material)tabData.tabObject;
                    TestViewer.Instance.SetViewerContent(tabData.tabType, tabData.tabObject);
                    break;
            }
			button.interactable = false;
		}

		public void DisableTab()
		{
			switch(tabData.tabType)
			{
				case ViewerBarTabType.Camera:
					break;
				case ViewerBarTabType.Texture:
					break;
                case ViewerBarTabType.Material:
                    break;
            }
			button.interactable = true;
		}

        public void CleanupTab()
        {
            switch (tabData.tabType)
            {
                case ViewerBarTabType.Camera:
                    break;
                case ViewerBarTabType.Texture:
                    Destroy(tabTexture); // TODO - Check for leaks here
                    break;
                case ViewerBarTabType.Material:
                    tabMaterial = null; // TODO - Check for leaks here
                    break;
            }
            button.interactable = true;
        }
    }
}
