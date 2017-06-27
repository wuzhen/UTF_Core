using System;
using UnityEngine;

namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // TestModelBase
    // - Lowest level TestModel class that all models derive from
    // - Hides most logic away from end user

    public abstract class TestModelBase : MonoBehaviour
    {
        public Type logic { get; set; } // Reference to the models logic type

        public abstract void SetLogic();
        
        [HideInInspector]
        public SettingsBase settings;

        public virtual void SetSettings()
        {
            settings = new SettingsBase();
        }
    }

    // ------------------------------------------------------------------------------------
    // TestModel
    // - Next level TestModel class that all user facing logics derive from
    // - Adds an abstraction layer for defining logic type

    public abstract class TestModel<L> : TestModelBase where L : TestLogicBase
    {
        // Set test logic type
        public override void SetLogic()
        {
            logic = typeof(L); // Set type
        }
    }

    // ------------------------------------------------------------------------------------
    // ResultsBase
    // - Base class for Results
    // - Hides ResultsDataCommon to ensure it is available

    [Serializable]
    public class ResultsBase
    {
        public ResultsDataCommon common; // Set automatically (mandatory)
    }

    // ------------------------------------------------------------------------------------
    // SettingsBase
    // - Base class for Settings
    // - Hides common settings to ensure they are always available

    [Serializable]
    public class SettingsBase
    {
        public enum WaitType { Frames, Seconds }

        public WaitType waitType = WaitType.Frames; // Type of measurement for waiting
        public float waitTimer = 0f; // Count of frames or seconds to wait before capture
        public float passFailThreshold = 0.1f; // Threshold for comparison pass/fail
    }
}
