using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphicsTestFramework
{
    public abstract class TestDisplayBase : MonoBehaviour
    {
        /// ------------------------------------------------------------------------------------
        /// Initial setup methods

        /*public virtual void SetName()
        {
            testTypeName = "Untitled Logic";
        }

        public void SetSuiteName(string name)
        {
            testSuiteName = name;
        }*/

        public abstract void SetLogic(TestLogicBase inputLogic);

        // Enable test viewer (if in View mode)
        // TODO - Revisit this when rewriting the TestViewer
        public virtual void EnableTestViewer()
        {
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log(this.GetType().Name + " enabling Test Viewer");
            ProgressScreen.Instance.SetState(false, ProgressType.LocalSave, "");
            TestViewer.Instance.SetTestViewerState(1, ViewerType.Default, null);
        }

        //public abstract void SetResultsType();
    }

    public abstract class TestDisplay<T> : TestDisplayBase where T : TestLogicBase
    {
        public T logic { get; set; }

        public abstract override void SetLogic(TestLogicBase inputLogic);
    }
}
