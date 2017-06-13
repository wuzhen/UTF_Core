using System;
using UnityEngine;

namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // Results Data Structures

    // Structure for results (Do not rename class. Class contents can be anything)
    [System.Serializable]
    public class ExampleResults : ResultsBase
    {
        public float SomeFloat; // Just some example data. Well use this for comparison.
        public int SomeInt; // Just some more example data (can save most types, including arrays)
    }

    // ------------------------------------------------------------------------------------
    // ExampleModel
    // - Contains settings for Example

    public class ExampleModel : TestModel<ExampleLogic>
    {
        // ------------------------------------------------------------------------------------
        // Settings

        [Serializable]
        public struct Settings
        {
            public float waitTime; // Time to wait
            public float passFailThreshold; // Threshold for comparison pass/fail

            public static Settings defaultSettings // Default settings
            {
                get
                {
                    return new Settings
                    {
                        waitTime = 0f,
                        passFailThreshold = 0.1f
                    };
                }
            }
        }

        [SerializeField]
        Settings m_Settings = Settings.defaultSettings;
        public Settings settings
        {
            get { return m_Settings; }
            set { m_Settings = value; }
        }
    }
}
