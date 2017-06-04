using UnityEngine;

namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // AverageFrameTimeDisplay
    // - Controls context logic for TestViewer and ResultsViewer

    public class AverageFrameTimeDisplay : TestDisplay<AverageFrameTimeLogic>
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
