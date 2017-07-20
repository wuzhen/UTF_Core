using System;
using System.Collections;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;

namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // FlipBookLogic
    // - Results: Captures a screenshot from models test camera
    // - Comparison: Gets average value of pixel differences between results screenshot and baseline

    public class FlipBookLogic : TestLogic<FlipBookModel, FlipBookDisplay, FlipBookResults, FlipBookSettings>
    {
        // ------------------------------------------------------------------------------------
        // Variables

        Camera dummyCamera;
        RenderTexture temporaryRt;
		//Texture2D resultFrame;
		Texture2D[] resultFrames;
		int captureCount;
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

        // Manage dummy camera when logic is initialized
        public override void SetupLogic()
        {
            if (dummyCamera == null) // Dummy camera isnt initialized
                dummyCamera = this.gameObject.AddComponent<Camera>(); // Create camera component
            dummyCamera.enabled = false; // Disable dummy camera
        }

        // First injection point for custom code. Runs before any test logic (optional override)
        // - Set up cameras and create RenderTexture
        public override void TestPreProcess()
        {
            var typedSettings = (FlipBookSettings)model.settings; // Set settings to local type
            Vector2 resolution = Vector2.zero; // Create vector2
            model.resolutionList.TryGetValue(typedSettings.frameResolution, out resolution); // Get resolution
			resultFrames = new Texture2D[typedSettings.framesToCapture];
            temporaryRt = new RenderTexture((int)resolution.x, (int)resolution.y, 24); // Get a temporary RenderTexture for blit operations
            SetupCameras(); // Setup cameras
            StartTest(); // Start test
        }

        // Logic for creating results data (mandatory override)
        public override IEnumerator ProcessResult()
        {
            var m_TempData = (FlipBookResults)GetResultsStruct(); // Get a results struct (mandatory)
            yield return WaitForTimer(); // Wait for timer
            var typedSettings = (FlipBookSettings)model.settings; // Set settings to local type
            typedSettings.captureCamera.targetTexture = temporaryRt; // Set capture cameras target texture to temporary RT (logic specific)


			float captureWaitTime = typedSettings.captureWaitType == SettingsBase.WaitType.Frames ? typedSettings.captureWaitFrames : typedSettings.captureWaitSeconds;
			do {
				doCapture = true; // Perform OnRenderImage logic (logic specific)
				yield return WaitForTimer (typedSettings.captureWaitType, captureWaitTime); // Wait for capture time to pass
			} while (captureCount < typedSettings.framesToCapture);

			m_TempData.resultFrames = resultFrames; // save to results data
            if (baselineExists) // Comparison (mandatory)
            {
                FlipBookResults referenceData = (FlipBookResults)DeserializeResults(ResultsIO.Instance.RetrieveBaseline(suiteName, testTypeName, m_TempData.common)); // Deserialize baseline data (mandatory)
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

        // Last injection point for custom code. Runs after all test logic.
        // - Disable camera
        public override void TestPostProcess()
        {
            dummyCamera.enabled = false; // Disable dummy camera
			temporaryRt.Release(); // Release main RT
			var typedSettings = (FlipBookSettings)model.settings; // Set settings to local type
			typedSettings.captureCamera.targetTexture = null; // Set target texture to null
			captureCount = 0; // Reset capture count
            EndTest(); // End test
        }

        // Logic for comparison process (mandatory)
        // TODO - Will use last run test model, need to get this for every call from Viewers?
        public override object ProcessComparison(ResultsBase baselineData, ResultsBase resultsData)
        {
            ComparisonData newComparison = new ComparisonData(); // Create new ComparisonData instance (mandatory)
            FlipBookResults baselineDataTyped = (FlipBookResults)baselineData; // Set baseline data to local type
            FlipBookResults resultsDataTyped = (FlipBookResults)resultsData; // Set results data to local type
            newComparison.baselineTex = baselineDataTyped.resultFrames[0]; // Convert baseline frame to Texture2D (logic specific)
            newComparison.resultsTex = resultsDataTyped.resultFrames[0]; // Convert result frame to Texture2D (logic specific)
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
				Debug.LogWarning (Time.realtimeSinceStartup);
                doCapture = false; // Reset
                var typedSettings = (FlipBookSettings)model.settings; // Set settings to local type
                Vector2 resolution = Vector2.zero; // Create vector2
                model.resolutionList.TryGetValue(typedSettings.frameResolution, out resolution); // Get resolution
                var rt1 = RenderTexture.GetTemporary((int)resolution.x, (int)resolution.y, 24, temporaryRt.format, RenderTextureReadWrite.sRGB); // Get a temporary RT for blitting to
				Graphics.Blit(temporaryRt, rt1); // Blit models camera to the RT
				resultFrames[captureCount] = Common.ConvertRenderTextureToTexture2D(activeTestEntry.testName + "_Result", rt1, resolution, typedSettings.textureFormat, typedSettings.filterMode); // Convert the resulting render texture to a Texture2D
                RenderTexture.ReleaseTemporary(rt1); // Release the temporary RT
                Console.Instance.Write(DebugLevel.Logic, MessageLevel.Log, this.GetType().Name + " completed blit operations for test " + activeTestEntry.testName); // Write to console
				captureCount++;// Increment the capture count
            }
        }

        // Prepare cameras for capture
        void SetupCameras()
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, this.GetType().Name + " is setting up cameras"); // Write to console
            var typedSettings = (FlipBookSettings)model.settings; // Set settings to local type
            if (dummyCamera == null) // Dummy camera isnt initialized
                dummyCamera = this.gameObject.AddComponent<Camera>(); // Create camera component
            dummyCamera.enabled = true; // Enable dummy camera
            if (typedSettings.captureCamera == null) // If no capture camera
            {
                FlipBookSettings settings = typedSettings; // Clone the settings
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
            resultFrames = null; // Null
        }
    }
}
