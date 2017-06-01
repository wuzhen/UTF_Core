using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphicsTestFramework
{
    public abstract class TestDisplayBase : MonoBehaviour
    {
        public GameObject resultsContextPrefab;

        /// ------------------------------------------------------------------------------------
        /// Initial setup methods

        public abstract void SetLogic(TestLogicBase inputLogic);

        public virtual void GetResultsContextObject()
        {
            string name = this.GetType().ToString();
            name = name.Replace("GraphicsTestFramework.", "").Replace("Display", "");
            name = "ResultsContext_" + name;
            resultsContextPrefab = (GameObject)Resources.Load(name);
        }

        public abstract void SetupResultsContext();

        // Enable test viewer (if in View mode)
        // TODO - Revisit this when rewriting the TestViewer
        public virtual void EnableTestViewer(object resultsObject)
        {
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log(this.GetType().Name + " enabling Test Viewer");
            ProgressScreen.Instance.SetState(false, ProgressType.LocalSave, "");
            TestViewer.Instance.SetTestViewerState(1, ViewerType.Default, null);
        }
    }

    public abstract class TestDisplay<T> : TestDisplayBase where T : TestLogicBase
    {
        public T logic { get; set; }

        public abstract override void SetLogic(TestLogicBase inputLogic);
    }
}
