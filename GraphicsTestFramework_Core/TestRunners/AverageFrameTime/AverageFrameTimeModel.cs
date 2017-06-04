using System;
using UnityEngine;

namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // Results Data Structures

    // Structure for results
    [System.Serializable]
    public class AverageFrameTimeResults : ResultsBase
    {
        public float avgFrameTime;
    }

    // ------------------------------------------------------------------------------------
    // AverageFrameTimeModel
    // - Contains settings for AverageFrameTime

    public class AverageFrameTimeModel : TestModel<AverageFrameTimeLogic> 
	{
        // ------------------------------------------------------------------------------------
        // Local Enums

        public enum TimingType { Seconds, Milliseconds, Ticks, Custom }

        // ------------------------------------------------------------------------------------
        // ID

        public override void SetID()
        {
            id = 002;
        }

        // ------------------------------------------------------------------------------------
        // Settings

        [Serializable]
        public struct Settings
        {
			public TimingType timingType; // Timing multiplier
			public float customTimingMultiplier; // Custom timing multiplier
            public int sampleFrames; // Amount of frames to sample
			public int waitFrames; // Amount of frames to wait
			public float passFailThreshold; // Threshold for comparison pass/fail

            public static Settings defaultSettings // Default settings
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

        [SerializeField]
        Settings m_Settings = Settings.defaultSettings;
        public Settings settings
        {
            get { return m_Settings; }
            set { m_Settings = value; }
        }
	}
}
