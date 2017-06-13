using System;
using System.Collections.Generic;
using UnityEngine;

namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // Results Data Structures

    // Structure for results
    [System.Serializable]
    public class FrameComparisonResults : ResultsBase
    {
        public string resultFrame;
    }

    // ------------------------------------------------------------------------------------
    // FrameComparisonModel
    // - Contains settings for FrameComparison

    public class FrameComparisonModel : TestModel<FrameComparisonLogic>
    {
        // ------------------------------------------------------------------------------------
        // Resolutions

        public enum Resolution
        {
            [Tooltip("640x360")] nHD,
            [Tooltip("960x540")] qHD,
            [Tooltip("1280x720")] HD,
            [Tooltip("1920x1080")] FullHD
        }

        static Dictionary<Resolution, Vector2> m_ResolutionList = new Dictionary<Resolution, Vector2>
        {
            //{#, typeof(ExampleModel) }, // We dont include ExampleModel here as it is only for reference
            {Resolution.nHD , new Vector2(640, 360) },
            {Resolution.qHD , new Vector2(960, 540) },
            {Resolution.HD , new Vector2(1280, 720) },
            {Resolution.FullHD , new Vector2(1920, 1080) },
        };

        public Dictionary<Resolution, Vector2> resolutionList { get { return m_ResolutionList; } }

        // ------------------------------------------------------------------------------------
        // Settings

        [Serializable]
        public struct Settings
        {
            public Camera captureCamera; //Reference to the camera used to capture
            public Resolution frameResolution; //Resolution of the frame capture
            public TextureFormat textureFormat; //Format of the frame capture
            public FilterMode filterMode; //Filter mode of the frame capture
            public int waitFrames; //Count of frames to wait before capture
            public float passFailThreshold; // Threshold for comparison pass/fail

            public static Settings defaultSettings // Default settings
            {
                get
                {
                    return new Settings
                    {
                        captureCamera = null,
                        frameResolution = Resolution.qHD,
                        textureFormat = TextureFormat.RGB24,
                        filterMode = FilterMode.Bilinear,
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
