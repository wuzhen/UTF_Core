using System.Collections;
using UnityEngine;

namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // FrameComparisonLogic
    // - Results: Captures a screenshot from models test camera
    // - Comparison: Gets average value of pixel differences between results screenshot and baseline

    public class FrameComparisonLogic : TestLogic<FrameComparisonModel, FrameComparisonDisplay, FrameComparisonResults>
    {
        // ------------------------------------------------------------------------------------
        // Variables

        Camera dummyCamera;
        RenderTexture temporaryRt;
        Texture2D resultsTexture;
        bool doCapture;        

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
            var m_TempData = (FrameComparisonResults)GetResultsStruct(); // Get a results struct (mandatory)
            for (int i = 0; i < model.settings.waitFrames; i++) // Wait for requested wait frame count (logic specific)
                yield return new WaitForEndOfFrame();
            model.settings.captureCamera.targetTexture = temporaryRt; // Set capture cameras target texture to temporary RT (logic specific)
            doCapture = true; // Perform OnRenderImage logic (logic specific)
            do { yield return null; } while (resultsTexture == null); // Wait for OnRenderImage logic to complete (logic specific)
            m_TempData.resultFrame = System.Convert.ToBase64String(resultsTexture.EncodeToPNG()); // Convert results texture to Base64 String and save to results data
            if (baselineExists) // Comparison (mandatory)
            {
                FrameComparisonResults referenceData = (FrameComparisonResults)DeserializeResults(ResultsIO.Instance.RetrieveBaseline(suiteName, testTypeName, m_TempData.common)); // Deserialize baseline data (mandatory)
                ComparisonData comparisonData = (ComparisonData)ProcessComparison(referenceData, m_TempData);  // Prrocess comparison (mandatory)
                if (comparisonData.DiffPercentage < model.settings.passFailThreshold)  // Pass/fail decision logic (logic specific)
                    m_TempData.common.PassFail = true;
                else
                    m_TempData.common.PassFail = false;
                comparisonData = null;  // Null comparison (mandatory)
            }
            Cleanup(); // Cleanup (logic specific)
            BuildResultsStruct(m_TempData); // Submit (mandatory)
        }

        // Logic for comparison process (mandatory)
        // TODO - Will use last run test model, need to get this for every call from Viewers?
        public override object ProcessComparison(ResultsBase baselineData, ResultsBase resultsData)
        {
            ComparisonData newComparison = new ComparisonData(); // Create new ComparisonData instance (mandatory)
            FrameComparisonResults baselineDataTyped = (FrameComparisonResults)baselineData;
            FrameComparisonResults resultsDataTyped = (FrameComparisonResults)resultsData;
            newComparison.baselineTex = Common.ConvertStringToTexture(resultsDataTyped.common.TestName + "_Reference", baselineDataTyped.resultFrame); // Convert baseline frame to Texture2D (logic specific)
            newComparison.resultsTex = Common.ConvertStringToTexture(resultsDataTyped.common.TestName + "_Results", resultsDataTyped.resultFrame); // Convert result frame to Texture2D (logic specific)
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
                resultsTexture = Common.ConvertRenderTextureToTexture2D(activeTestEntry.testName + "_Result", rt1, model.settings.frameResolution, model.settings.textureFormat, model.settings.filterMode); // Convert the resulting render texture to a Texture2D
                model.settings.captureCamera.targetTexture = null; // Set target texture to null
                RenderTexture.ReleaseTemporary(rt1); // Release the temporary RT
                temporaryRt.Release(); // Release main RT
                Console.Instance.Write(DebugLevel.Logic, MessageLevel.Log, this.GetType().Name + " completed blit operations for test " + activeTestEntry.testName); // Write to console
            }
        }

        // Prepare cameras for capture
        void SetupCameras()
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, this.GetType().Name + " is setting up cameras"); // Write to console
            if (dummyCamera == null) // Dummy camera isnt initialized
                dummyCamera = this.gameObject.AddComponent<Camera>(); // Create camera component
            if (model.settings.captureCamera == null) // If no capture camera
            {
                FrameComparisonModel.Settings settings = model.settings; // Clone the settings
                settings.captureCamera = Camera.main; // Attempt to set capture camera to main
                if (settings.captureCamera == null) // If no main camera found
                {
                    Camera[] cams = FindObjectsOfType<Camera>(); // Find all cameras
                    settings.captureCamera = cams[cams.Length-1]; // Set to last in found array so avoid setting to UI or dummy cameras
                }
                if(settings.captureCamera == null) // If still not found
                {
                    settings.captureCamera = dummyCamera; // Set to dummy camera as fallback
                    Console.Instance.Write(DebugLevel.Critical, MessageLevel.LogWarning, "Frame Comparison test found no camera inside test "+activeTestEntry.testName); // Write to console
                }
                model.settings = settings; // Set settings back
            }
                
        }

        // Cleanup cameras after test finishes
        void Cleanup()
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, this.GetType().Name + " is cleaning up"); // Write to console
            resultsTexture = null; // Null
        }
    }
}
