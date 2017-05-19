using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphicsTestFramework
{
    public class Master : MonoBehaviour
    {
        public enum DebugMode { None, Messages, DummyData, OnlyMessages };
        public DebugMode debugMode;
        public float applicationVersion; //Store this elsewhere
        public string buildDirectory; //Store this elsewhere?
        public string buildName; //Store this elsewhere?
        //public SystemData systemData;

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

		public SystemData GetSystemData()
		{
			SystemData output = new SystemData();
			output.UnityVersion = Application.unityVersion;
			output.AppVersion = applicationVersion.ToString();
			output.Platform = Application.platform.ToString();
			output.API = SystemInfo.graphicsDeviceType.ToString();
			return output;
		}

		public DateTime GetSystemTime(){
			return System.DateTime.UtcNow;
		}

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
           // systemData = GetSystemData();
        }

    }

	[System.Serializable]
	public class SystemData
	{
		public string UnityVersion; //Dont remove
		public string AppVersion; //Dont remove
		public string Platform; //Dont remove
		public string API; //Dont remove 
	}
}
