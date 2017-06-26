using System;
using UnityEngine;

namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // Results Data Structures

    // Structure for results
    [Serializable]
    public class AverageFrameTimeResults : ResultsBase
    {
        public float avgFrameTime;
    }

    // ------------------------------------------------------------------------------------
    // Settings Data Structures

    // Structure for settings
    [Serializable]
    public class AverageFrameTimeSettings : SettingsBase
    {
        // Local Enums
        public enum TimingType { Seconds, Milliseconds, Ticks, Custom }

        public TimingType timingType = TimingType.Milliseconds; // Timing multiplier
        public float customTimingMultiplier = 1f; // Custom timing multiplier
        public int sampleFrames = 32; // Amount of frames to sample
    }

    // ------------------------------------------------------------------------------------
    // AverageFrameTimeModel
    // - Contains settings for AverageFrameTime

    public class AverageFrameTimeModel : TestModel<AverageFrameTimeLogic> 
	{
        [SerializeField]
        AverageFrameTimeSettings m_Settings = new AverageFrameTimeSettings();

        public override void SetSettings()
        {
            settings = m_Settings;
        }
    }
}
