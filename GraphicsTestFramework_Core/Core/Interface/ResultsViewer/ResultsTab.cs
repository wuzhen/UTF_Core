using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GraphicsTestFramework
{
    public class ResultsTab : MonoBehaviour
    {
        public enum TabType { Suite, TestType }

        public TabType tabType;
        public int index;
        public Text label;

        public void Setup(TabType type, int id, string title)
        {
            tabType = type;
            index = id;
            label.text = title;
            switch(tabType)
            {
                case TabType.Suite:
                    GetComponent<Button>().onClick.AddListener(delegate { ResultsViewer.Instance.SelectSuite(index); });
                    break;
                case TabType.TestType:
                    GetComponent<Button>().onClick.AddListener(delegate { ResultsViewer.Instance.SelectType(index); });
                    break;
            }
        }
    }
}
