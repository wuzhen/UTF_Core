using System;
using UnityEngine;

namespace GraphicsTestFramework
{
    public class Configuration : MonoBehaviour
    {
        // ------------------------------------------------------------------------------------
        // Variables

        // Singleton
        private static Configuration _Instance = null;
        public static Configuration Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = (Configuration)FindObjectOfType(typeof(Configuration));
                return _Instance;
            }
        }

        public Settings settings = new Settings();

        [Serializable]
        public class Settings
        {
            public bool testviewerOnAutomationTestFail;
        }
    }
}

