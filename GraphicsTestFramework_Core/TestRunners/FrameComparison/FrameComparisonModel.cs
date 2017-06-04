using System;
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
        // ID

        public override void SetID()
        {
            id = 001;
        }

        // ------------------------------------------------------------------------------------
        // Settings

        [Serializable]
        public struct Settings
        {
            public Camera captureCamera; //Reference to the camera used to capture
            public Vector2 frameResolution; //Resolution of the frame capture
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
                        frameResolution = new Vector2(720, 405),
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
