using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;

namespace GraphicsTestFramework
{
    public class FrameComparisonLogic : TestLogic<FrameComparisonModel>
    {
        /// ------------------------------------------------------------------------------------
        /// Logic specific variables

        Camera dummyCamera;
        RenderTexture temporaryRt;
        Texture2D activeReferenceTexture;
        bool doCapture;
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

        /// ------------------------------------------------------------------------------------
        /// Logic specififc results class

        ResultsData m_TempData; //Dont remove or edit (write result data into this)

        //Structure for results
        [System.Serializable]
        public class ResultsData
        {
            public ResultsDataCommon common; //Dont remove (set automatically)
            public float DiffPercentage;
            public string resultFrame;
            public string comparisonFrame;
        }

        //Structure for comparison
        /*[System.Serializable]
        public class ComparisonData
        {
            public float DiffPercentage;
            //public string comparisonFrame;
        }*/

        // Setup the results structs every test (Dont edit)
        public override void SetupResultsStructs()
        {
            ResultsData newResultsData = new ResultsData();
            newResultsData.common = Common.GetCommonResultsData();
            newResultsData.common.SceneName = activeTestInfo.SceneName;
            newResultsData.common.TestName = activeTestInfo.TestName;
            activeResultData = newResultsData;
        }

        /// ------------------------------------------------------------------------------------
        /// Initial setup methods

        // Set name
        public override void SetName()
        {
            testTypeName = "Frame Comparison";
        }

        //Set model
        public override void SetModel(TestModel inputModel)
        {
            model = (FrameComparisonModel)inputModel;
        }

        //Set reference to logic script for this model
        public override void SetDisplayType()
        {
            displayType = typeof(FrameComparisonDisplay);
        }

        public override void SetDisplayObject(TestDisplayBase inputDisplay)
        {
            displayObject = (FrameComparisonDisplay)inputDisplay;
        }

        //Set results type
        public override void SetResultsType()
        {
            resultsType = typeof(ResultsData);
        }

        /// ------------------------------------------------------------------------------------
        /// Main logic flow methods (overrides) 

        // First injection point for custom code. Runs before any test logic. 
        // - Set up cameras and create RenderTexture
        public override void TestPreProcess()
        {
            temporaryRt = new RenderTexture((int)model.settings.frameResolution.x, (int)model.settings.frameResolution.y, 24);
            if (!SetupCameras())
            {
                Debug.LogWarning("Camera reference missing. Aborting");
            }
            StartTest();
        }

        // Logic for creating baseline data
        public override IEnumerator ProcessBaseline()
        {
            m_TempData = (ResultsData)GetResultsStruct();
            for (int i = 0; i < model.settings.waitFrames; i++)
                yield return new WaitForEndOfFrame();
            model.settings.captureCamera.targetTexture = temporaryRt;
            doCapture = true;
            do { yield return null; } while (m_TempData.resultFrame == null);
            BuildResultsStruct(m_TempData);
        }

        // Logic for creating results data
        public override IEnumerator ProcessResult()
        {
            m_TempData = (ResultsData)GetResultsStruct();
            for (int i = 0; i < model.settings.waitFrames; i++)
                yield return new WaitForEndOfFrame();
            ResultsData referenceData = (ResultsData)DeserializeResults(ResultsIO.Instance.RetrieveBaseline(testSuiteName, testTypeName, m_TempData.common));
            //ResultsData referenceData = (ResultsData)baseline;
            activeReferenceTexture = Common.BuildTextureFromByteArray(referenceData.common.TestName + "_Reference", referenceData.resultFrame, model.settings.frameResolution, model.settings.textureFormat, model.settings.filterMode);
            do { yield return null; } while (activeReferenceTexture == null);
            model.settings.captureCamera.targetTexture = temporaryRt;
            doCapture = true;
            do { yield return null; } while (m_TempData.comparisonFrame == null);
            CleanupCameras(); // Need to reset cameras rects here
            if (m_TempData.DiffPercentage < model.settings.passFailThreshold)
                m_TempData.common.PassFail = true;
            else
                m_TempData.common.PassFail = false;
            BuildResultsStruct(m_TempData);
        }

        /*public ComparisonData ProcessComparison()
        {

        }*/

        /// ------------------------------------------------------------------------------------
        /// Custom test logic methods
        /// No relation to base class or any other test type

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Graphics.Blit(source, destination);
            if (doCapture)
            {
                doCapture = false;
                var rt1 = RenderTexture.GetTemporary((int)model.settings.frameResolution.x, (int)model.settings.frameResolution.y, 24, temporaryRt.format, RenderTextureReadWrite.sRGB);
                Graphics.Blit(temporaryRt, rt1); //Blit camera to the RT
                Texture2D resultsTex = Common.ConvertRenderTextureToTexture2D(activeTestInfo.TestName + "_Result", rt1, model.settings.frameResolution, model.settings.textureFormat, model.settings.filterMode);
                m_TempData.resultFrame = System.Convert.ToBase64String(resultsTex.EncodeToPNG());
                if (stateType == StateType.CreateResults)
                {
                    var rt2 = RenderTexture.GetTemporary((int)model.settings.frameResolution.x, (int)model.settings.frameResolution.y, 24, temporaryRt.format, RenderTextureReadWrite.sRGB);
                    material.SetTexture("_ReferenceTex", activeReferenceTexture);
                    Graphics.Blit(rt1, rt2, material, 0);
                    Texture2D comparisonTex = Common.ConvertRenderTextureToTexture2D(activeTestInfo.TestName + "_Comparison", rt2, model.settings.frameResolution, model.settings.textureFormat, model.settings.filterMode);
                    m_TempData.DiffPercentage = Common.GetTextureComparisonValue(comparisonTex);
                    m_TempData.comparisonFrame = System.Convert.ToBase64String(comparisonTex.EncodeToPNG());
                    RenderTexture.ReleaseTemporary(rt2);
                    //Destroy(rt2);
                }
                else if (stateType == StateType.CreateBaseline)
                {
                    m_TempData.DiffPercentage = 0.0f;
                    m_TempData.comparisonFrame = "-";
                    //Destroy(rt2);
                }
                if (Master.Instance.debugMode == Master.DebugMode.Messages)
                    Debug.Log(this.GetType().Name + " completed blit operations for test " + activeTestInfo.TestName);
                model.settings.captureCamera.targetTexture = null;
                RenderTexture.ReleaseTemporary(rt1); //Release the RT
                temporaryRt.Release();
                //Destroy(rt1);
            }
        }

        //Prepare cameras for capture
        bool SetupCameras()
        {
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log(this.GetType().Name + " is setting up cameras");
            if (dummyCamera == null)
            {
                dummyCamera = this.gameObject.AddComponent<Camera>();
                dummyCamera.rect = new Rect(0, 0, 1, 1);
                //TODO - This still samples a fullscreen blit //dummyCamera.pixelRect = new Rect(0, 0, model.settings.frameResolution.x, model.settings.frameResolution.y);
            }
            if (model.settings.captureCamera)
            {
                model.settings.captureCamera.rect = new Rect(0, 0, 1, 1);
                //TODO - This still samples a fullscreen blit //model.settings.captureCamera.pixelRect = new Rect(0, 0, model.settings.frameResolution.x, model.settings.frameResolution.y);
                return true;
            }
            else
                return false;
        }

        // Cleanup cameras after test finishes
        void CleanupCameras()
        {
            if (dummyCamera)
                Destroy(dummyCamera);
            if (model.settings.captureCamera)
                model.settings.captureCamera.rect = new Rect(0, 0, 1, 1);
        }
    }
}
