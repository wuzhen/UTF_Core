using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphicsTestFramework
{
    public class AverageFrameTimeDisplay : TestDisplay<AverageFrameTimeLogic>
    {
        /// ------------------------------------------------------------------------------------
        /// Initial setup methods

        //Set logic
        public override void SetLogic(TestLogicBase inputLogic)
        {
            logic = (AverageFrameTimeLogic)inputLogic;
        }

        /// ------------------------------------------------------------------------------------
        /// TestViewer related methods
        /// TODO - Revisit this when rewriting the TestViewer

        // Enable and setup the test viewer
        // TODO - Revisit this when rewriting the TestViewer
        /*public override void EnableTestViewer()
        {
            // TODO
        }*/

        /// ------------------------------------------------------------------------------------
        /// ResultsViewer related methods
        /// TODO - Revisit this when rewriting the ResultsViewer
        /// 

        public override void SetupResultsContext()
        {
            
        }
    }
}
