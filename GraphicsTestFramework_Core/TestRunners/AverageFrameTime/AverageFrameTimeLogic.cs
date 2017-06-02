using System.Collections;
using UnityEngine;

namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // AverageFrameTimeLogic
    // - Results: Samples a number of frames and returns an average
    // - Comparison: Compares average frame time to baseline

    public class AverageFrameTimeLogic : TestLogic<AverageFrameTimeModel, AverageFrameTimeDisplay>
	{
        // ------------------------------------------------------------------------------------
        // Variables

        float time;
		int samples;

        // ------------------------------------------------------------------------------------
        // Results Data Structures
        
        // Structure for results
        [System.Serializable]
        public class ResultsData
        {
            public ResultsDataCommon common; // Set automatically (mandatory)
            public float avgFrameTime;
        }

        // Structure for comparison
        [System.Serializable]
        public class ComparisonData
        {
            public float delta;
        }

        // ------------------------------------------------------------------------------------
        // Execution Overrides

        // Logic for creating results data (mandatory override)
        public override IEnumerator ProcessResult()
        {
            m_TempData = (ResultsData)GetResultsStruct(); // Get a results struct (mandatory)
			for (int i = 0; i < model.settings.waitFrames; i++) // Wait for requested wait frame count (logic specific)
                yield return new WaitForEndOfFrame();
            Timestamp(false); // Perform a timestamp (logic specific)
            for (int i = 0; i < model.settings.sampleFrames; i++) // Wait for requested sample frame count (logic specific)
                yield return new WaitForEndOfFrame();
			m_TempData.avgFrameTime = Timestamp(true); // Perform a timestamp (logic specific)
            if (baselineExists) // Comparison (mandatory)
            {
                ResultsData referenceData = (ResultsData)DeserializeResults(ResultsIO.Instance.RetrieveBaseline(activeTestEntry.suiteName, testTypeName, m_TempData.common)); // Deserialize baseline data (mandatory)
                ComparisonData comparisonData = ProcessComparison(referenceData, m_TempData);  // Prrocess comparison (mandatory)
                if (comparisonData.delta < model.settings.passFailThreshold)  // Pass/fail decision logic (logic specific)
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
            newComparison.delta = resultsData.avgFrameTime - baselineData.avgFrameTime; // Perform comparison logic (logic specific)
            return newComparison; // Return (mandatory)
        }

        // ------------------------------------------------------------------------------------
        // Test Type Specific Methods

        // Calculate time and frames since last Timestamp and return an average
        float Timestamp(bool debug)
		{
			float multiplier = 1; // Create a multiplier
			switch(model.settings.timingType) // Set multiplier based on model settings
			{
				case AverageFrameTimeModel.TimingType.Seconds:
					multiplier = 1;
					break;
				case AverageFrameTimeModel.TimingType.Milliseconds:
					multiplier = 1000;
					break;
				case AverageFrameTimeModel.TimingType.Ticks:
					multiplier = 10000000;
					break;
				case AverageFrameTimeModel.TimingType.Custom:
					multiplier = model.settings.customTimingMultiplier;
					break;
			}
			float currentTime = Time.realtimeSinceStartup * multiplier; // Get current time
			int currentSamples = Time.frameCount; // Get current samples
			float elapsedTime = currentTime - time; // Get elapsed time since last Timestamp
			int elapsedSamples = currentSamples - samples; // Get elapsed samples since last Timestamp
			time = currentTime; // Reset time
			samples = currentSamples; // Reset samples
            Console.Instance.Write(DebugLevel.Logic, MessageLevel.Log, this.GetType().Name + " completed test with " + elapsedSamples + " samples with average frametime of " + elapsedTime / (float)elapsedSamples); // Write to console
			return elapsedTime / (float)elapsedSamples; // Return
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
