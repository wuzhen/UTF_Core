using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;

namespace GraphicsTestFramework
{
	public class ExampleLogic : TestLogic<ExampleModel> //Set type here to matching model type
	{
		/// ------------------------------------------------------------------------------------
        /// Logic specific variables
        
        float timeWaited; //Used for example

        /// ------------------------------------------------------------------------------------
        /// Logic specififc results class

        ResultsData m_TempData; //Dont remove (write result data into this)

        //Structure for results (Do not rename class. Class contents can be anything)
        [System.Serializable]
		public class ResultsData
		{
			public ResultsDataCommon common; //Dont remove (set automatically)
			public string SomeString;
			public float SomeFloat;
			public int SomeInt;
			public bool SomeBool;
            public string[] SomeStringArray;
		}

        /// ------------------------------------------------------------------------------------
        /// Initial setup methods
        /// All test logic types must override these methods
        
        //Set name
        public override void SetName()
        {
            testTypeName = "Example";
        }

        //Set model
        public override void SetModel(TestModel inputModel)
        {
            model = (ExampleModel)inputModel;
        }

        //Set results type
        public override void SetResultsType()
        {
            resultsType = typeof(ResultsData);
        }

        /// ------------------------------------------------------------------------------------
        /// Main logic flow methods (overrides) 
        /// 
        /// Mandatory overrides:
        /// - ProcessBaseline
        /// - ProcessResult
        /// 
        /// Optional overrides:
        /// - TestPreProcess
        /// - TestPostProcess
        /// 
        /// These method calls are already wrapped in debugs and as such do not require debugs inside them
        /// However, should you want to add further debugs please wrap them in:
        /// "if (Master.Instance.debugMode == Master.DebugMode.Messages)"


        // First injection point for custom code. Runs before any test logic.
        /*public override void TestPreProcess()
        {
            // Custom test pre-processing logic here
            StartTest();
        }*/

        // Logic for creating baseline data
        public override IEnumerator ProcessBaseline()
        {
            m_TempData = (ResultsData)GetResultsStruct(); //Must get results struct and cast to this logics results type
            if (timeWaited < model.settings.waitTime) //Check if waited time specified by active test options
            { 
                timeWaited += Time.deltaTime; //Contune waiting
                yield return null;
            }
            m_TempData = GetDummyData(m_TempData.common);
            BuildResultsStruct(m_TempData); //Must pass results struct to be built at the end of this logic
        }

        // Logic for creating results data
        public override IEnumerator ProcessResult()
		{
			m_TempData = (ResultsData)GetResultsStruct(); //Must get results struct and cast to this logics results type
            if (timeWaited < model.settings.waitTime) //Check if waited time specified by active test options
            { 
				timeWaited += Time.deltaTime; //Contune waiting
				yield return null;
			}
            m_TempData = GetDummyData(m_TempData.common);
            BuildResultsStruct(m_TempData); //Must pass results struct to be built at the end of this logic
        }

        // Last injection point for custom code. Runs after all test logic.
        /*public override void TestPostProcess()
        {
            // Custom test post-processing logic here
            EndTest();
        }*/

        /// ------------------------------------------------------------------------------------
        /// Custom test logic
        /// No relation to base class or any other test type
        /// In this example this is needed to generate dummy data to pass as results

        ResultsData GetDummyData (ResultsDataCommon common)
        {
            ResultsData output = new ResultsData();
            output.common = common;
            output.SomeString = "Test String";
            output.SomeFloat = UnityEngine.Random.value;
            output.SomeInt = Mathf.RoundToInt(output.SomeFloat);
            output.SomeStringArray = new string[2] { "Entry1", "Entry2" };
            if (output.SomeFloat > 0.5f)
                output.SomeBool = true;
            else
                output.SomeBool = false;
            return output;
        }

        /// ------------------------------------------------------------------------------------
        /// TestViewer related methods
        /// TODO - Revisit this when rewriting the TestViewer
        /// TestViewer will default to a default setup with no tabs
        /// If you wish to use tabs please refer to this override method in "FrameComparisonLogic.cs"
        /// WARNING : This logic is subject to immediate change

        /*public override void EnableTestViewer()
        {
            TestViewer.Instance.SetTestViewerState(1, ViewerType.Default, null);
        }*/

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
    }
}
