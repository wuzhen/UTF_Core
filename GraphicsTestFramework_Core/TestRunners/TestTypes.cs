using System;
using System.Collections.Generic;

namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // TestTypes
    // - Master dictionary of all test types
    // - Used to get a test type

    public static class TestTypes
    {
        static Dictionary< int, Type> m_TypeList = new Dictionary<int, Type>
        {
            //{#, typeof(ExampleModel) }, // We dont include ExampleModel here as it is only for reference
            {0, typeof(AverageFrameTimeModel) },
            {1, typeof(FrameComparisonModel) },
        };

        // ------------------------------------------------------------------------------------
        // Get Data

        // Get a type from its index
        public static Type GetTypeFromIndex(int index)
        {
            return m_TypeList[index]; // Return requested type
        }
    }

}
