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

        ResultsData m_TempData; //Dont remove (write result data into this)

        //Structure for results
        [System.Serializable]
        public class ResultsData
        {
            public ResultsDataCommon common; //Dont remove (set automatically)
            public float avgFrameTime;
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

        //Set results type
        public override void SetResultsType()
        {
            resultsType = typeof(ResultsData);
        }

		/// ------------------------------------------------------------------------------------
        /// Main logic flow methods (overrides) 

        // Logic for creating baseline data
        public override IEnumerator ProcessBaseline()
        {
			m_TempData = (ResultsData)GetResultsStruct();
            for (int i = 0; i < model.settings.waitFrames; i++)
                yield return new WaitForEndOfFrame();
            Timestamp(false);
            for (int i = 0; i < model.settings.sampleFrames; i++)
                yield return new WaitForEndOfFrame();
			m_TempData.avgFrameTime = Timestamp(true);
            BuildResultsStruct(m_TempData);
        }

        // Logic for creating results data
        public override IEnumerator ProcessResult()
        {
            m_TempData = (ResultsData)GetResultsStruct();
			ResultsData referenceData = GenerateBaselineData(ResultsIO.Instance.RetrieveBaseline(testSuiteName, testTypeName, m_TempData.common));
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

		/// ------------------------------------------------------------------------------------
        /// METHODS BELOW ARE NOT CONTEXT SENSITIVE
        /// DO NOT EDIT

        /// ------------------------------------------------------------------------------------
        /// Results
        /// TODO - Attempt to move even more stuff from from here to the abstract class

        // Setup the results structs every test
        public override void SetupResultsStructs()
        {
            ResultsData newResultsData = new ResultsData();
            newResultsData.common = Common.GetCommonResultsData();
            newResultsData.common.SceneName = activeTestInfo.SceneName;
            newResultsData.common.TestName = activeTestInfo.TestName;
            activeResultData = newResultsData;
        }

        // Deserialize ResultsIOData(string arrays) to ResultsData(class)
        ResultsData GenerateBaselineData(ResultsIOData resultsIOData)
        {
            ResultsData resultData = new ResultsData(); //blank results data
            resultData.common = new ResultsDataCommon(); //blank common data

            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            FieldInfo[] commonFields = typeof(ResultsDataCommon).GetFields(bindingFlags);
            FieldInfo[] customFields = typeof(ResultsData).GetFields(bindingFlags);

            List<string> commonDataRaw = resultsIOData.resultsRow[0].resultsColumn.GetRange(0, commonFields.Length * 2);
            List<string> resultsDataRaw = resultsIOData.resultsRow[0].resultsColumn.GetRange(commonFields.Length * 2, resultsIOData.resultsRow[0].resultsColumn.Count - (commonFields.Length * 2));

            for (int f = 0; f < customFields.Length; f++)
            {
                if (f == 0)
                {
                    //do the common class
                    for (int cf = 0; cf < commonFields.Length; cf++)
                    {
                        string value = commonDataRaw[(cf * 2) + 1];
                        FieldInfo fieldInfo = resultData.common.GetType().GetField(commonFields[cf].Name);
                        fieldInfo.SetValue(resultData.common, Convert.ChangeType(value, fieldInfo.FieldType));
                    }
                }
                else
                {
                    var value = resultsDataRaw[(f * 2) - 1];
                    FieldInfo fieldInfo = resultData.GetType().GetField(customFields[f].Name);
                    if (fieldInfo.FieldType.IsArray) // This handles arrays
                    {
                        Type type = resultData.GetType().GetField(customFields[f].Name).FieldType.GetElementType();
                        GenerateGenericArray(fieldInfo, resultData.GetType(), resultData, type, value);
                    }
                    else // Non array types
                    {
                        fieldInfo.SetValue(resultData, Convert.ChangeType(value, fieldInfo.FieldType));
                    }
                }
            }
            return resultData;
        }
    }
}
