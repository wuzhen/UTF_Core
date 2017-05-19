using System;
using UnityEngine;

namespace GraphicsTestFramework
{
    public class ExampleModel : TestModel
    {
        //Struct for test options. One instance per test.
        [Serializable]
        public struct Settings
        {
            public float waitTime;

            public static Settings defaultSettings
            {
                get
                {
                    return new Settings
                    {
                        waitTime = 0f
                    };
                }
            }
        }

        //Reference to logic script for this model
        public override void SetLogic()
        {
            logic = typeof(ExampleLogic);
        }

        //Get reference to logic script for this model
        public override TestLogicBase GetLogic()
        {
            return new ExampleLogic();
        }

        [SerializeField]
        Settings m_Settings = Settings.defaultSettings;
        public Settings settings
        {
            get { return m_Settings; }
            set { m_Settings = value; }
        }

        public void Reset()
        {
            m_Settings = Settings.defaultSettings;
        }
    }
}
