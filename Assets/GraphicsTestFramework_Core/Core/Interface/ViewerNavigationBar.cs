using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GraphicsTestFramework
{
	public class ViewerNavigationBar : MonoBehaviour 
	{
		public Text breadcrumbLabel;
		bool isHidden = true;
		RectTransform thisRect;
		float navigationBarHeight;
		public RectTransform tabContentRect;
		public GameObject tabPrefab;
		int currentTab;
		ViewerBarTab[] tabs;

		private static ViewerNavigationBar _Instance = null;
        public static ViewerNavigationBar Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = (ViewerNavigationBar)FindObjectOfType(typeof(ViewerNavigationBar));
                return _Instance;
            }
        }

		void Start()
		{
			thisRect = GetComponent<RectTransform>();
		}

		public void OnClickShowHide()
		{
			isHidden = !isHidden;
			UpdatePosition();
		}

		public void UpdatePosition()
		{
			switch(isHidden)
			{
				case false:
					thisRect.anchoredPosition = new Vector2(0, -navigationBarHeight);
					break;
				case true:
					thisRect.anchoredPosition = new Vector2(0,0);
					break;
			}
		}

		public void OnClickTab(int index)
		{
			currentTab = index;
			for(int i = 0; i < tabs.Length; i++)
			{
				if(i != index)
				{
					tabs[i].DisableTab();
				}
				else
				{
					tabs[i].EnableTab();
				}
			}
		}

		public void UpdateNavigationBar(ViewerType type, object contextObject)
        {
			if(tabs != null)
			{
				foreach(ViewerBarTab tab in tabs)
					Destroy(tab.gameObject);
                tabs = null;
			}
            TestRunner.TestEntry currentTest = TestRunner.Instance.GetCurrentTestEntry();
            breadcrumbLabel.text = currentTest.suiteName+" - "+currentTest.sceneName+" - "+currentTest.typeName+" - "+currentTest.testName;
			switch(type)
			{
				case ViewerType.Default:
					navigationBarHeight = thisRect.sizeDelta.y / 2;
					UpdatePosition();
					break;
				case ViewerType.DefaultTabs:
					navigationBarHeight = thisRect.sizeDelta.y;
					UpdatePosition();
					ViewerBarTabData[] tabDatas = (ViewerBarTabData[])contextObject;
					tabs = new ViewerBarTab[tabDatas.Length];
					for(int i = 0; i < tabs.Length; i++)
					{
						GameObject go = Instantiate(tabPrefab, tabContentRect, false);
						ViewerBarTab newTab = go.GetComponent<ViewerBarTab>();
						tabs[i] = newTab;
						newTab.SetupTab(i, tabDatas[i]);
					}
					tabs[0].EnableTab();
					break;
			}
        }
	}
}
