using System;
using UnityEngine;

namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // Master
    // - System data structures and returns
    // - Maintains persistence of other logic objects

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

        //Data
        public static string applicationVersion = "1.0b1";

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
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Getting system data"); // Write to console
            SystemData output = new SystemData(); // Create new class instance
			output.UnityVersion = Application.unityVersion; // Get Unity version
			output.AppVersion = applicationVersion.ToString(); // Get application version
            output.OS = SystemInfo.operatingSystem; // Get OS
            output.Device = SystemInfo.deviceModel + "|" + SystemInfo.graphicsDeviceName + "|" + SystemInfo.processorType;
			output.Platform = Application.platform.ToString(); // Get platform
			output.API = SystemInfo.graphicsDeviceType.ToString(); // Get graphics device type
			return output; // Return
		}

        // Get the current system time
		public DateTime GetSystemTime()
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Getting system time"); // Write to console
            return DateTime.UtcNow; // Return current DateTime
		}

        // ------------------------------------------------------------------------------------
        // Application

        // Exit the application
        public void ExitApplication()
        {
            Application.Quit(); // Quit
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; // If editor stop play mode
#endif
        }

        // ------------------------------------------------------------------------------------
        // Editor

#if UNITY_EDITOR

        // Setup for build
        [UnityEditor.MenuItem("RuntimeTestFramework/Prepare Build")]
        public static void Prebuild()
        {
            SuiteManager.Instance.GenerateSceneList(); // Create suite structure
            int platformCount = Enum.GetNames(typeof(BuildTargetGroup)).Length; // Get platform count
            for(int i = 0; i < platformCount; i++) // Iterate all platforms
                UnityEditor.PlayerSettings.SetApplicationIdentifier((BuildTargetGroup)i, "com.UnityTechnologies.RuntimeTestFramework"); // Set bundle identifiers
            UnityEditor.PlayerSettings.bundleVersion = applicationVersion; // Set application version
        }

#endif

    }

    // ------------------------------------------------------------------------------------
    // Global Data Structures

    // System data class
    [System.Serializable]
	public class SystemData
	{
		public string UnityVersion;
		public string AppVersion;
        public string OS;
        public string Device;
		public string Platform;
		public string API;
	}
}
