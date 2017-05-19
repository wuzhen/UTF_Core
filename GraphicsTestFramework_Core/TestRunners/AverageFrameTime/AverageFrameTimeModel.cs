using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphicsTestFramework
{
	public class AverageFrameTimeModel : TestModel 
	{
		public enum TimingType { Seconds, Milliseconds, Ticks, Custom }
		//Struct for test options. One instance per test.
        [Serializable]
        public struct Settings
        {
			public TimingType timingType;
			public float customTimingMultiplier;
            public int sampleFrames;
			public int waitFrames;	
			public float passFailThreshold;

            public static Settings defaultSettings
            {
                get
                {
                    return new Settings
                    {
                        timingType = TimingType.Milliseconds,
						customTimingMultiplier = 1,
						sampleFrames = 32,
						waitFrames = 0,
						passFailThreshold = 0.1f
                    };
                }
            }
        }

        //Set reference to logic script for this model
        public override void SetLogic()
        {
            logic = typeof(AverageFrameTimeLogic);
        }

        //Get reference to logic script for this model
        public override TestLogicBase GetLogic()
        {
            return new AverageFrameTimeLogic();
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
