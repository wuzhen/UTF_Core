using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;

namespace GraphicsTestFramework
{
	public class AverageFrameTimeLogic : TestLogic<AverageFrameTimeModel> 
	{
		/// ------------------------------------------------------------------------------------
        /// Logic specific variables

		float time;
		int samples;

		/// ------------------------------------------------------------------------------------
        /// Logic specififc results class

        ResultsData m_TempData; //Dont remove or edit (write result data into this)

        //Structure for results
        [System.Serializable]
        public class ResultsData
        {
            public ResultsDataCommon common; //Dont remove (set automatically)
            public float avgFrameTime;
        }

        //Structure for comparison
        [System.Serializable]
        public class ComparisonData
        {
            public float delta;
        }

        // Setup the results structs every test  (Dont edit)
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
            testTypeName = "Average Frame Time";
        }

        // Set model
        public override void SetModel(TestModel inputModel)
        {
            model = (AverageFrameTimeModel)inputModel;
        }

        // Set display
        public override void SetDisplayType()
        {
            displayType = typeof(AverageFrameTimeDisplay);
        }

        //Set results type
        public override void SetResultsType()
        {
            resultsType = typeof(ResultsData);
        }

		/// ------------------------------------------------------------------------------------
        /// Main logic flow methods (overrides) 

        // Logic for creating results data
        public override IEnumerator ProcessResult()
        {
            m_TempData = (ResultsData)GetResultsStruct();
			for (int i = 0; i < model.settings.waitFrames; i++)
                yield return new WaitForEndOfFrame();
            Timestamp(false);
            for (int i = 0; i < model.settings.sampleFrames; i++)
                yield return new WaitForEndOfFrame();
			m_TempData.avgFrameTime = Timestamp(true);
            //Comparison
            if (stateType == StateType.CreateResults)
            {
                ResultsData referenceData = (ResultsData)DeserializeResults(ResultsIO.Instance.RetrieveBaseline(testSuiteName, testTypeName, m_TempData.common));
                ComparisonData comparisonData = ProcessComparison(referenceData, m_TempData);
                if (m_TempData.avgFrameTime - comparisonData.delta < model.settings.passFailThreshold)
                    m_TempData.common.PassFail = true;
                else
                    m_TempData.common.PassFail = false;
                comparisonData = null; // TODO - Check for leaks here
            }
            //Finalise
            BuildResultsStruct(m_TempData);
        }

        // TODO - Will use last run test model, need to get this for every call from Viewers? :/
        public ComparisonData ProcessComparison(ResultsData baselineData, ResultsData resultsData)
        {
            ComparisonData newComparison = new ComparisonData();
            newComparison.delta = resultsData.avgFrameTime - baselineData.avgFrameTime;
            return newComparison;
        }

        /// ------------------------------------------------------------------------------------
        /// Custom test logic methods
        /// No relation to base class or any other test type

        float Timestamp(bool debug)
		{
			float multiplier = 1;
			switch(model.settings.timingType)
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
			float currentTime = Time.realtimeSinceStartup * multiplier;
			int currentSamples = Time.frameCount;
			float elapsedTime = currentTime - time;
			int elapsedSamples = currentSamples - samples;
			time = currentTime;
			samples = currentSamples;
			if(debug && Master.Instance.debugMode == Master.DebugMode.Messages)
			{
				Debug.Log(this.GetType().Name+" completed test with "+elapsedSamples+" samples");
				Debug.Log(this.GetType().Name+" completed test with frametime of "+elapsedTime / (float)elapsedSamples);
			}
			return elapsedTime / (float)elapsedSamples;
		}
    }
}
