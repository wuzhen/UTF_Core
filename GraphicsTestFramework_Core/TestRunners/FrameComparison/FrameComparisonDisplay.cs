using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphicsTestFramework
{
    public class FrameComparisonDisplay : TestDisplay<FrameComparisonLogic>
    {
        /// ------------------------------------------------------------------------------------
        /// Initial setup methods

        //Set logic
        public override void SetLogic(TestLogicBase inputLogic)
        {
            logic = (FrameComparisonLogic)inputLogic;
        }

        

        /// ------------------------------------------------------------------------------------
        /// TestViewer related methods
        /// TODO - Revisit this when rewriting the TestViewer

        // Enable and setup the test viewer
        // TODO - Revisit this when rewriting the TestViewer
        public override void EnableTestViewer()
        {
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log(this.GetType().Name + " enabling Test Viewer");
            object contextObject = new object();
            FrameComparisonLogic.ResultsData currentResults = (FrameComparisonLogic.ResultsData)logic.activeResultData;
            switch (logic.stateType)
            {
                case TestLogicBase.StateType.CreateBaseline:
                    ViewerBarTabData[] tabs = new ViewerBarTabData[2];
                    for (int i = 0; i < tabs.Length; i++)
                        tabs[i] = new ViewerBarTabData();
                    tabs[0].tabName = "Live Camera";
                    tabs[0].tabType = ViewerBarTabType.Camera;
                    tabs[0].tabCamera = logic.model.settings.captureCamera;
                    tabs[1].tabName = "Results Texture";
                    tabs[1].tabType = ViewerBarTabType.Texture;
                    tabs[1].tabTexture = currentResults.resultFrame;
                    tabs[1].textureResolution = logic.model.settings.frameResolution;
                    contextObject = tabs;
                    break;
                case TestLogicBase.StateType.CreateResults:
                    ViewerBarTabData[] tabs2 = new ViewerBarTabData[4];
                    for (int i = 0; i < tabs2.Length; i++)
                        tabs2[i] = new ViewerBarTabData();
                    tabs2[0].tabName = "Live Camera";
                    tabs2[0].tabType = ViewerBarTabType.Camera;
                    tabs2[0].tabCamera = logic.model.settings.captureCamera;
                    tabs2[1].tabName = "Results Texture";
                    tabs2[1].tabType = ViewerBarTabType.Texture;
                    tabs2[1].tabTexture = currentResults.resultFrame;
                    tabs2[1].textureResolution = logic.model.settings.frameResolution;
                    tabs2[2].tabName = "Comparison Texture";
                    tabs2[2].tabType = ViewerBarTabType.Texture;
                    tabs2[2].tabTexture = currentResults.comparisonFrame;
                    tabs2[2].textureResolution = logic.model.settings.frameResolution;
                    tabs2[3].tabName = "Baseline Texture";
                    tabs2[3].tabType = ViewerBarTabType.Texture;
                    FrameComparisonLogic.ResultsData baselineData = (FrameComparisonLogic.ResultsData)logic.DeserializeResults(ResultsIO.Instance.RetrieveBaseline(logic.testSuiteName, logic.testTypeName, currentResults.common));
                    tabs2[3].tabTexture = baselineData.resultFrame;
                    tabs2[3].textureResolution = logic.model.settings.frameResolution;
                    contextObject = tabs2;
                    break;
            }
            ProgressScreen.Instance.SetState(false, ProgressType.LocalSave, ""); // TODO - Move this so its abstracted
            TestViewer.Instance.SetTestViewerState(1, ViewerType.DefaultTabs, contextObject);
        }
    }
}
