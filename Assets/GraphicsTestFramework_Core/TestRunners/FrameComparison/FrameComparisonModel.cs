using System;
using UnityEngine;

namespace GraphicsTestFramework
{
    public class FrameComparisonModel : TestModel
    {
        //Struct for test options. One instance per test.
        [Serializable]
        public struct Settings
        {
            public Camera captureCamera; //Reference to the camera used to capture
            public Vector2 frameResolution; //Resolution of the frame capture
            public TextureFormat textureFormat; //Format of the frame capture
            public FilterMode filterMode; //Filter mode of the frame capture
            public int waitFrames; //Count of frames to wait before capture
            public float passFailThreshold;

            public static Settings defaultSettings
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

        //Set reference to logic script for this model
        public override void SetLogic()
        {
            logic = typeof(FrameComparisonLogic);
        }

        //Get reference to logic script for this model
        public override TestLogicBase GetLogic()
        {
            return new FrameComparisonLogic();
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
