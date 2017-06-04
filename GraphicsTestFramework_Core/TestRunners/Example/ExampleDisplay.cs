using UnityEngine;

namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // ExampleDisplay
    // - Controls context logic for TestViewer and ResultsViewer

    public class ExampleDisplay : TestDisplay<ExampleLogic>
    {
        // ------------------------------------------------------------------------------------
        // TestViewer

        // Enable and setup the test viewer
        // TODO - Total rewrite
        //public override void EnableTestViewer()
        //{
        //    // TODO
        //}

        // ------------------------------------------------------------------------------------
        // ResultsViewer

        // Setup the results context object
        // TODO - Total rewrite
        public override void SetupResultsContext(GameObject contextObject, ResultsEntry inputEntry)
        {

        }
    }
}
