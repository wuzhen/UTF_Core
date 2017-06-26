using System;
using System.Collections.Generic;
using UnityEngine;

namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // Results Data Structures

    // Structure for results
    [Serializable]
    public class FrameComparisonResults : ResultsBase
    {
        public string resultFrame;
    }

    // ------------------------------------------------------------------------------------
    // Settings Data Structures

    // Structure for settings
    [Serializable]
    public class FrameComparisonSettings : SettingsBase
    {
        public Camera captureCamera; //Reference to the camera used to capture
        public FrameResolution frameResolution = FrameResolution.qHD; //Resolution of the frame capture
        public TextureFormat textureFormat = TextureFormat.RGB24; //Format of the frame capture
        public FilterMode filterMode = FilterMode.Bilinear; //Filter mode of the frame capture
    }

    // ------------------------------------------------------------------------------------
    // FrameComparisonModel
    // - Contains settings for FrameComparison

    public class FrameComparisonModel : TestModel<FrameComparisonLogic>
    {
        public Dictionary<FrameResolution, Vector2> resolutionList { get { return Common.frameResolutionList; } }

        [SerializeField]
        FrameComparisonSettings m_Settings = new FrameComparisonSettings();

        public override void SetSettings()
        {
            settings = m_Settings;
        }
    }
}
