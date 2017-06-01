using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GraphicsTestFramework
{
    public class FrameComparisonDisplay : TestDisplay<FrameComparisonLogic>
    {
        /// ------------------------------------------------------------------------------------
        /// Logic specific variables

        Material m_Material;
        public Material material
        {
            get
            {
                if (m_Material == null)
                    m_Material = new Material(Shader.Find("Hidden/FrameComparison")) { hideFlags = HideFlags.DontSave };
                return m_Material;
            }
        }

        void SetupMaterial(Texture2D baselineTex, Texture2D resultsTex)
        {
            material.SetTexture("_ReferenceTex", baselineTex);
            material.SetTexture("_MainTex", resultsTex);
        }

        FrameComparisonLogic.ComparisonData comparisonData;

        public RawImage resultsContextImage;
        public Button button0;
        public Button button1;
        public Button button2;

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
        // TODO - All the fetch logic is dirty
        public override void EnableTestViewer(object resultsObject)
        {
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log(this.GetType().Name + " enabling Test Viewer");
            object contextObject = new object();

            FrameComparisonLogic.ResultsData inputResults = (FrameComparisonLogic.ResultsData)resultsObject;
            GetComparisonData(inputResults.common);

            switch (logic.stateType)
            {
			case TestLogicBase.StateType.CreateBaseline:
				ViewerBarTabData[] tabs = new ViewerBarTabData[2];
				for (int i = 0; i < tabs.Length; i++)
					tabs [i] = new ViewerBarTabData ();
					tabs [0].tabName = "Live Camera";
					tabs [0].tabType = ViewerBarTabType.Camera;
					tabs [0].tabObject = logic.model.settings.captureCamera;
					tabs [1].tabName = "Results Texture";
					tabs [1].tabType = ViewerBarTabType.Texture;
					FrameComparisonLogic.ResultsData localResultData = (FrameComparisonLogic.ResultsData)logic.activeResultData;
					tabs[1].tabObject = Common.BuildTextureFromByteArray("Tab_ResultsFrame", localResultData.resultFrame);
                    tabs[1].textureResolution = logic.model.settings.frameResolution;
                    contextObject = tabs;
                    break;
                case TestLogicBase.StateType.CreateResults:
                    ViewerBarTabData[] tabs2 = new ViewerBarTabData[4];
                    for (int i = 0; i < tabs2.Length; i++)
                        tabs2[i] = new ViewerBarTabData();
                    tabs2[0].tabName = "Live Camera";
                    tabs2[0].tabType = ViewerBarTabType.Camera;
                    tabs2[0].tabObject = logic.model.settings.captureCamera;
                    tabs2[1].tabName = "Results Texture";
                    tabs2[1].tabType = ViewerBarTabType.Texture;
                    tabs2[1].tabObject = comparisonData.resultsTex;
                    tabs2[1].textureResolution = logic.model.settings.frameResolution;
                    tabs2[2].tabName = "Comparison Texture";
                    tabs2[2].tabType = ViewerBarTabType.Material;
                    SetupMaterial(comparisonData.baselineTex, comparisonData.resultsTex);
                    tabs2[2].tabObject = material;
                    tabs2[2].textureResolution = logic.model.settings.frameResolution;
                    tabs2[3].tabName = "Baseline Texture";
                    tabs2[3].tabType = ViewerBarTabType.Texture;
                    tabs2[3].tabObject = comparisonData.baselineTex;
                    tabs2[3].textureResolution = logic.model.settings.frameResolution;
                    contextObject = tabs2;
                    break;
            }
            ProgressScreen.Instance.SetState(false, ProgressType.LocalSave, ""); // TODO - Move this so its abstracted
            TestViewer.Instance.SetTestViewerState(1, ViewerType.DefaultTabs, contextObject);
        }

        void GetComparisonData(ResultsDataCommon inputCommon)
        {
            Debug.LogWarning("GetComparisonData: "+inputCommon.DateTime + " - " + inputCommon.SceneName + " - " + inputCommon.TestName);
            ResultsIOData baselineFetch = ResultsIO.Instance.RetrieveBaseline(logic.testSuiteName, logic.testTypeName, inputCommon);
            comparisonData = new FrameComparisonLogic.ComparisonData();
            if (baselineFetch != null)
            {
                FrameComparisonLogic.ResultsData resultsData = (FrameComparisonLogic.ResultsData)logic.DeserializeResults(ResultsIO.Instance.RetrieveResult(logic.testSuiteName, logic.testTypeName, inputCommon));
                FrameComparisonLogic.ResultsData baselineData = (FrameComparisonLogic.ResultsData)logic.DeserializeResults(baselineFetch);
                comparisonData = logic.ProcessComparison(baselineData, resultsData);
            }
            else
                Debug.LogWarning("Base fetch failed");
        }

        /// ------------------------------------------------------------------------------------
        /// ResultsViewer related methods
        /// TODO - Revisit this when rewriting the ResultsViewer
        /// 

        public override void SetupResultsContext(GameObject contextObject, ResultsEntry inputEntry)
        {
            FrameComparisonLogic.ResultsData inputResults = (FrameComparisonLogic.ResultsData)logic.DeserializeResults(inputEntry.resultsData);

            GetComparisonData(inputResults.common);
            Debug.LogWarning("SetupResultsContext: " + inputResults.common.DateTime + " - " + inputResults.common.SceneName + " - " + inputResults.common.TestName);

            ResultsContext context = contextObject.GetComponent<ResultsContext>();
            button0 = context.objects[0].GetComponent<Button>();
            button0.onClick.AddListener(delegate { SetTextureContext(0); });
            button1 = context.objects[1].GetComponent<Button>();
            button1.onClick.AddListener(delegate { SetTextureContext(1); });
            button2 = context.objects[2].GetComponent<Button>();
            button2.onClick.AddListener(delegate { SetTextureContext(2); });
            context.objects[3].GetComponent<Text>().text = "Results";
            context.objects[4].GetComponent<Text>().text = "Comparison";
            context.objects[5].GetComponent<Text>().text = "Baseline";
            resultsContextImage = context.objects[6].GetComponent<RawImage>();
            context.objects[7].GetComponent<Text>().text = comparisonData.DiffPercentage.ToString();
            SetTextureContext(0);
        }

        public void SetTextureContext(int context)
        {
            button0.interactable = true;
            button1.interactable = true;
            button2.interactable = true;
            switch (context)
            {
                case 0:
                    button0.interactable = false;
                    resultsContextImage.material = null;
                    resultsContextImage.texture = comparisonData.resultsTex;
                    break;
                case 1:
                    button1.interactable = false;
                    SetupMaterial(comparisonData.baselineTex, comparisonData.resultsTex);
                    resultsContextImage.material = material;
                    break;
                case 2:
                    button2.interactable = false;
                    resultsContextImage.material = null;
                    resultsContextImage.texture = comparisonData.baselineTex;
                    break;
            }
        }
    }
}
