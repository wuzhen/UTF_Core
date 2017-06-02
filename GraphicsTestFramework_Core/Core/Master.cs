using System;
using UnityEngine;

namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // Master
    // - System data structures and returns
    // - Maintains persistence of other logic objects
    // - Debug controls

    public class Master : MonoBehaviour
    {
        // ------------------------------------------------------------------------------------
        // Variables

        // Singleton
        private static Master _Instance = null;
        public static Master Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = (Master)FindObjectOfType(typeof(Master));
                return _Instance;
            }
        }

        // Debug
        public enum DebugMode { None, Messages, DummyData, OnlyMessages };
        public DebugMode debugMode;

        //Data
        public float applicationVersion; // TODO - Store this elsewhere
        public string buildDirectory; // TODO - Store this elsewhere?
        public string buildName; // TODO - Store this elsewhere?

        // ------------------------------------------------------------------------------------
        // Setup

        // On Awake
        private void Awake()
        {
            DontDestroyOnLoad(gameObject); // Set this object to DontDestroy
        }

        // ------------------------------------------------------------------------------------
        // Get System Data

        // Get SystemData to use for building ResultsCommon
        public SystemData GetSystemData()
		{
			SystemData output = new SystemData(); // Create new class instance
			output.UnityVersion = Application.unityVersion; // Get Unity version
			output.AppVersion = applicationVersion.ToString(); // Get application version
			output.Platform = Application.platform.ToString(); // Get platform
			output.API = SystemInfo.graphicsDeviceType.ToString(); // Get graphics device type
			return output; // Return
		}

        // Get the current system time
		public DateTime GetSystemTime()
        {
			return DateTime.UtcNow; // Return current DateTime
		}
    }

    // ------------------------------------------------------------------------------------
    // Global Data Structures

    // System data class
    [System.Serializable]
	public class SystemData
	{
		public string UnityVersion;
		public string AppVersion;
		public string Platform;
		public string API;
	}
}
