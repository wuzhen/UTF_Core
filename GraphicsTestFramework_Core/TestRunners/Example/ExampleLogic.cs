using System.Collections;
using UnityEngine;

namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // ExampleLogic
    // - Serves only as example of logic custom setup

    public class ExampleLogic : TestLogic<ExampleModel, ExampleDisplay> // Set types here for matching: < ModelType , DisplayType >
	{
        // ------------------------------------------------------------------------------------
        // Variables

        float timeWaited; // Used for example

        // ------------------------------------------------------------------------------------
        // Results Data Structures

        // Structure for results (Do not rename class. Class contents can be anything)
        [System.Serializable]
		public class ResultsData
		{
			public ResultsDataCommon common; // Set automatically (mandatory)
            public float SomeFloat; // Just some example data. Well use this for comparison.
			public int SomeInt; // Just some more example data (can save most types, including arrays)
		}

        // Structure for results (Do not rename class. Class contents can be anything)
        [System.Serializable]
        public class ComparisonData
        {
            public float SomeFloatDiff; // Just some example data. Well use this for comparison.
        }

        // ------------------------------------------------------------------------------------
        // Execution Overrides
        // 
        // Mandatory overrides:
        // - ProcessResult
        //
        // Mandatory methods:
        // - Process Comparison (TODO - Make this an override)
        //
        // Optional overrides:
        // - TestPreProcess
        // - TestPostProcess
        // 
        // These method calls are already wrapped in debugs and as such do not require debugs inside them
        // However, should you want to add further debugs please use Console.Write()

        // First injection point for custom code. Runs before any test logic.
        public override void TestPreProcess()
        {
            // Custom test pre-processing logic here
            StartTest(); // Start test (mandatory if overriding this method)
        }

        // Logic for creating results data
        public override IEnumerator ProcessResult()
		{
			m_TempData = (ResultsData)GetResultsStruct(); // Must get results struct and cast to this logics results type (mandatory)
            if (timeWaited < model.settings.waitTime) // Check if waited time specified by active test options (logic specific)
            { 
				timeWaited += Time.deltaTime; // Contune waiting (logic specific)
                yield return null;
			}
            m_TempData = GetDummyData(m_TempData.common); // Just get some dummy data for the example (logic specific)
            if (baselineExists) // Comparison (mandatory)
            {
                ResultsData referenceData = (ResultsData)DeserializeResults(ResultsIO.Instance.RetrieveBaseline(activeTestEntry.suiteName, testTypeName, m_TempData.common)); // Deserialize baseline data (mandatory)
                ComparisonData comparisonData = ProcessComparison(referenceData, m_TempData);  // Prrocess comparison (mandatory)
                if (comparisonData.SomeFloatDiff < model.settings.passFailThreshold)  // Pass/fail decision logic (logic specific)
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
            newComparison.SomeFloatDiff = resultsData.SomeFloat - baselineData.SomeFloat; // Perform comparison logic (logic specific)
            return newComparison; // Return (mandatory)
        }

        // Last injection point for custom code. Runs after all test logic.
        public override void TestPostProcess()
        {
            // Custom test post-processing logic here
            EndTest(); // End test (mandatory if overriding this method)
        }

        // ------------------------------------------------------------------------------------
        // Test Type Specific Methods

        // Just get some dummy result data for the example
        ResultsData GetDummyData (ResultsDataCommon common)
        {
            ResultsData output = new ResultsData();
            output.common = common;
            output.SomeFloat = UnityEngine.Random.value;
            output.SomeInt = Mathf.RoundToInt(output.SomeFloat);
            return output;
        }

        // The following methods are (unfortunately) currently needed in every test logic script (due to strong typing)
        // TODO - More work on abstracting these

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
