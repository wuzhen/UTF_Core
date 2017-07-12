using System;
using System.Collections.Generic;
using UnityEngine;

namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // Results Data Structures

    // Structure for results
    [Serializable]
    public class FlipBookResults : ResultsBase
    {
		public Texture2D[] resultFrames;
    }

    // ------------------------------------------------------------------------------------
    // Settings Data Structures

    // Structure for settings
    [Serializable]
    public class FlipBookSettings : SettingsBase
    {
        public Camera captureCamera; //Reference to the camera used to capture
		public int framesToCapture; //The amount of frames to capture
		public WaitType captureWaitType; //The format of the time between captures, will be restricted to frames or seconds via custom inspector
		public int captureWaitFrames;//The time in frames between captures
		public float captureWaitSeconds;//The time in seconds between captures
        public FrameResolution frameResolution; //Resolution of the frame capture
        public TextureFormat textureFormat; //Format of the frame capture
		public FilterMode filterMode; //Filter mode used for the capture rames

        public static FlipBookSettings defaultSettings
        {
            get
            {
                return new FlipBookSettings
                {
                    waitType = WaitType.Frames, // Type of measurement for waiting
                    waitFrames = 1, // Count of frames to wait before capture
                    passFailThreshold = 0.1f, // Threshold for comparison pass/fail
                    captureCamera = null, //Reference to the camera used to capture
					framesToCapture = 4, // Number of images to capture
					captureWaitType = WaitType.Frames,// Type of timing to use between captures
					captureWaitFrames = 1,// Amount of frames to wait between each capture
                    frameResolution = FrameResolution.qHD, //Resolution of the frame capture
                    textureFormat = TextureFormat.RGB24, //Format of the frame capture
					filterMode = FilterMode.Bilinear //Filter mode used for the capture rames
                };
            }
        }
    }

    // ------------------------------------------------------------------------------------
    // FlipBookModel
    // - Contains settings for FlipBook

    public class FlipBookModel : TestModel<FlipBookLogic>
    {
        public Dictionary<FrameResolution, Vector2> resolutionList { get { return Common.frameResolutionList; } }

        // Exposed settings
        [SerializeField]
        FlipBookSettings m_Settings = FlipBookSettings.defaultSettings;

        // Set the exposed settings to the internal
        public override void SetSettings()
        {
            settings = m_Settings;
        }

		// Get/Set public settings
		public FlipBookSettings p_Settings
		{
			get
			{
				return m_Settings;
			}
		}

    }
}
