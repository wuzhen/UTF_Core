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
        //Texture2D activeReferenceTexture;
        bool doCapture;

        /// ------------------------------------------------------------------------------------
        /// Logic specififc results class

        ResultsData m_TempData; //Dont remove or edit (write result data into this)

        //Structure for results
        [System.Serializable]
        public class ResultsData
        {
            public ResultsDataCommon common; //Dont remove (set automatically)
            public string resultFrame;
        }

        //Structure for comparison
        [System.Serializable]
        public class ComparisonData
        {
            public float DiffPercentage;
            public Texture2D baselineTex;
            public Texture2D resultsTex;
        }

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

        // Logic for creating results data
        public override IEnumerator ProcessResult()
        {
            m_TempData = (ResultsData)GetResultsStruct();
            for (int i = 0; i < model.settings.waitFrames; i++)
                yield return new WaitForEndOfFrame();
            model.settings.captureCamera.targetTexture = temporaryRt;
            doCapture = true;
            do { yield return null; } while (m_TempData.resultFrame == null);
            CleanupCameras(); // Need to reset cameras rects here
            ResultsIOData ioData = ResultsIO.Instance.RetrieveBaseline(testSuiteName, testTypeName, m_TempData.common);
            if (ioData != null)
            {
                ResultsData referenceData = (ResultsData)DeserializeResults(ioData);
                ComparisonData comparison = ProcessComparison(referenceData, m_TempData);
                if (comparison.DiffPercentage < model.settings.passFailThreshold)
                    m_TempData.common.PassFail = true;
                else
                    m_TempData.common.PassFail = false;
                comparison = null; // TODO - Check for leaks here
            }
            BuildResultsStruct(m_TempData);
        }

        // TODO - Will use last run test model, need to get this for every call from Viewers? :/
        public ComparisonData ProcessComparison(ResultsData baselineData, ResultsData resultsData)
        {
            ComparisonData newComparison = new ComparisonData();
            newComparison.baselineTex = Common.BuildTextureFromByteArray(baselineData.common.TestName + "_Reference", baselineData.resultFrame, model.settings.frameResolution, model.settings.textureFormat, model.settings.filterMode);
            newComparison.resultsTex = Common.BuildTextureFromByteArray(resultsData.common.TestName + "_Results", resultsData.resultFrame, model.settings.frameResolution, model.settings.textureFormat, model.settings.filterMode);
            newComparison.DiffPercentage = Common.GetTextureComparisonValue(newComparison.baselineTex, newComparison.resultsTex);
            return newComparison;
        }

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
