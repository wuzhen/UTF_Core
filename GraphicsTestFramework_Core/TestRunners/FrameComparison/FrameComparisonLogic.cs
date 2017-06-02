using System.Collections;
using UnityEngine;

namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // FrameComparisonLogic
    // - Results: Captures a screenshot from models test camera
    // - Comparison: Gets average value of pixel differences between results screenshot and baseline

    public class FrameComparisonLogic : TestLogic<FrameComparisonModel, FrameComparisonDisplay>
    {
        // ------------------------------------------------------------------------------------
        // Variables

        Camera dummyCamera;
        RenderTexture temporaryRt;
        bool doCapture;

        // ------------------------------------------------------------------------------------
        // Results Data Structures

        // Structure for results
        [System.Serializable]
        public class ResultsData
        {
            public ResultsDataCommon common; // Set automatically (mandatory)
            public string resultFrame;
        }

        // Structure for comparison
        [System.Serializable]
        public class ComparisonData
        {
            public float DiffPercentage;
            public Texture2D baselineTex;
            public Texture2D resultsTex;
        }

        // ------------------------------------------------------------------------------------
        // Execution Overrides

        // First injection point for custom code. Runs before any test logic (optional override)
        // - Set up cameras and create RenderTexture
        public override void TestPreProcess()
        {
            temporaryRt = new RenderTexture((int)model.settings.frameResolution.x, (int)model.settings.frameResolution.y, 24); // Get a temporary RenderTexture for blit operations
            SetupCameras(); // Setup cameras
            StartTest(); // Start test
        }

        // Logic for creating results data (mandatory override)
        public override IEnumerator ProcessResult()
        {
            m_TempData = (ResultsData)GetResultsStruct(); // Get a results struct (mandatory)
            for (int i = 0; i < model.settings.waitFrames; i++) // Wait for requested wait frame count (logic specific)
                yield return new WaitForEndOfFrame();
            model.settings.captureCamera.targetTexture = temporaryRt; // Set capture cameras target texture to temporary RT (logic specific)
            doCapture = true; // Perform OnRenderImage logic (logic specific)
            do { yield return null; } while (m_TempData.resultFrame == null); // Wait for OnRenderImage logic to complete (logic specific)
            CleanupCameras(); // Reset camera rects (logic specific)
            if (baselineExists) // Comparison (mandatory)
            {
                ResultsData referenceData = (ResultsData)DeserializeResults(ResultsIO.Instance.RetrieveBaseline(activeTestEntry.suiteName, testTypeName, m_TempData.common)); // Deserialize baseline data (mandatory)
                ComparisonData comparisonData = ProcessComparison(referenceData, m_TempData);  // Prrocess comparison (mandatory)
                if (comparisonData.DiffPercentage < model.settings.passFailThreshold)  // Pass/fail decision logic (logic specific)
                    m_TempData.common.PassFail = true;
                else
                    m_TempData.common.PassFail = false;
                comparisonData = null;  // Null comparison (mandatory)
            }
            BuildResultsStruct(m_TempData); // Submit (mandatory)
        }

        // Logic for comparison process (mandatory)
        // TODO - Will use last run test model, need to get this for every call from Viewers?
        public ComparisonData ProcessComparison(ResultsData baselineData, ResultsData resultsData)
        {
            ComparisonData newComparison = new ComparisonData(); // Create new ComparisonData instance (mandatory)
            newComparison.baselineTex = Common.ConvertStringToTexture(baselineData.common.TestName + "_Reference", baselineData.resultFrame); // Convert baseline frame to Texture2D (logic specific)
            newComparison.resultsTex = Common.ConvertStringToTexture(resultsData.common.TestName + "_Results", resultsData.resultFrame); // Convert result frame to Texture2D (logic specific)
            newComparison.DiffPercentage = Common.GetTextureComparisonValue(newComparison.baselineTex, newComparison.resultsTex); // Calculate diff percentage (logic specific)
            return newComparison; // Return (mandatory)
        }

        // ------------------------------------------------------------------------------------
        // Test Type Specific Methods

        // Called on render
        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Graphics.Blit(source, destination); // Blit source to destination for Deferred
            if (doCapture) // If running blit operations
            {
                doCapture = false; // Reset
                var rt1 = RenderTexture.GetTemporary((int)model.settings.frameResolution.x, (int)model.settings.frameResolution.y, 24, temporaryRt.format, RenderTextureReadWrite.sRGB); // Get a temporary RT for blitting to
                Graphics.Blit(temporaryRt, rt1); // Blit models camera to the RT
                Texture2D resultsTex = Common.ConvertRenderTextureToTexture2D(activeTestEntry.testName + "_Result", rt1, model.settings.frameResolution, model.settings.textureFormat, model.settings.filterMode); // Convert the resultying render texture to a Texture2D
                m_TempData.resultFrame = System.Convert.ToBase64String(resultsTex.EncodeToPNG()); // Convert to Base64 String and save to results data
                Console.Instance.Write(DebugLevel.Logic, MessageLevel.Log, this.GetType().Name + " completed blit operations for test " + activeTestEntry.testName); // Write to console
                model.settings.captureCamera.targetTexture = null; // Set target texture to null
                RenderTexture.ReleaseTemporary(rt1); // Release the temporary RT
                temporaryRt.Release(); // Release main RT
            }
        }

        // Prepare cameras for capture
        void SetupCameras()
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, this.GetType().Name + " is setting up cameras"); // Write to console
            if (dummyCamera == null) // Dummy camera isnt initialized
                dummyCamera = this.gameObject.AddComponent<Camera>(); // Create camera component
        }

        // Cleanup cameras after test finishes
        void CleanupCameras()
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, this.GetType().Name + " is cleaning up cameras"); // Write to console
            if (dummyCamera) // If dummy camera exists
                Destroy(dummyCamera); // Destroy it
        }

        // ------------------------------------------------------------------------------------
        // --------------------------- DO NOT EDIT BELOW HERE ---------------------------------
        // ------------------------------------------------------------------------------------

        ResultsData m_TempData; // Current results data (Dont remove or edit)

        // Setup the results structs every test (Dont remove or edit)
        public override void SetupResultsStructs()
        {
            ResultsData newResultsData = new ResultsData();
            newResultsData.common = Common.GetCommonResultsData();
            newResultsData.common.SceneName = activeTestEntry.sceneName;
            newResultsData.common.TestName = activeTestEntry.testName;
            activeResultData = newResultsData;
        }

        //Set and initialize results type (Dont remove or edit)
        public override void SetResults()
        {
            resultsType = typeof(ResultsData);
            ResultsData newData = new ResultsData();
            newData.common = new ResultsDataCommon();
            activeResultData = newData;
        }
    }
}
