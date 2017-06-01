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

        //Set model
        public override void SetModel(TestModel inputModel)
        {
            model = (AverageFrameTimeModel)inputModel;
        }

        //Set reference to logic script for this model
        public override void SetDisplayType()
        {
            displayType = typeof(AverageFrameTimeDisplay);
        }

        public override void SetDisplayObject(TestDisplayBase inputDisplay)
        {
            displayObject = (AverageFrameTimeDisplay)inputDisplay;
        }

        //Set results type
        public override void SetResultsType()
        {
            resultsType = typeof(ResultsData);
        }

		/// ------------------------------------------------------------------------------------
        /// Main logic flow methods (overrides) 

        // Logic for creating baseline data
        /*public override IEnumerator ProcessBaseline()
        {
			m_TempData = (ResultsData)GetResultsStruct();
            for (int i = 0; i < model.settings.waitFrames; i++)
                yield return new WaitForEndOfFrame();
            Timestamp(false);
            for (int i = 0; i < model.settings.sampleFrames; i++)
                yield return new WaitForEndOfFrame();
			m_TempData.avgFrameTime = Timestamp(true);
            BuildResultsStruct(m_TempData);
        }*/

        // Logic for creating results data
        public override IEnumerator ProcessResult()
        {
            m_TempData = (ResultsData)GetResultsStruct();
			ResultsData referenceData = (ResultsData)DeserializeResults(ResultsIO.Instance.RetrieveBaseline(testSuiteName, testTypeName, m_TempData.common));
			for (int i = 0; i < model.settings.waitFrames; i++)
                yield return new WaitForEndOfFrame();
            Timestamp(false);
            for (int i = 0; i < model.settings.sampleFrames; i++)
                yield return new WaitForEndOfFrame();
			m_TempData.avgFrameTime = Timestamp(true);
			if(m_TempData.avgFrameTime - referenceData.avgFrameTime < model.settings.passFailThreshold)
				m_TempData.common.PassFail = true;
			else
				m_TempData.common.PassFail = false;
            BuildResultsStruct(m_TempData);
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
