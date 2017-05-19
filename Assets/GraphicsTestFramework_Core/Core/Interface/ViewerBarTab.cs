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
					TestViewer.Instance.SetViewerContent(tabData.tabType, tabData.tabCamera);
					break;
				case ViewerBarTabType.Texture:
					tabTexture = Common.BuildTextureFromByteArray("Temporary Tab Texture", tabData.tabTexture, tabData.textureResolution, TextureFormat.RGB24, FilterMode.Bilinear);
					TestViewer.Instance.SetViewerContent(tabData.tabType, tabTexture);
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
					Destroy(tabTexture);
					break;
			}
			button.interactable = true;
		}
	}
}
