using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // Global Enums

    public enum ProgressType { LocalLoad, LocalSave, CloudLoad, CloudSave }

    // ------------------------------------------------------------------------------------
    // ProgressScreen
    // - Overlay for loading messages during menus

    public class ProgressScreen : MonoBehaviour 
	{
        // ------------------------------------------------------------------------------------
        // Variables

        // Singleton
        private static ProgressScreen _Instance = null;
        public static ProgressScreen Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = (ProgressScreen)FindObjectOfType(typeof(ProgressScreen));
                return _Instance;
            }
        }

        // References
		public Discs discs;
		public Text messageText;
		public GameObject progressObject;

        // ------------------------------------------------------------------------------------
        // State & Context

        // Set state
        public void SetState(bool active, ProgressType type, string message)
		{
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Setting progress screen state to "+active); // Write to console
            progressObject.SetActive(active); // Set active
			if(active == true) // If active
			{
				discs.localLoad.SetActive(false); // Disable all loading discs
				discs.localSave.SetActive(false);
				discs.cloudLoad.SetActive(false);
				discs.cloudSave.SetActive(false);
				messageText.text = message; // Set message

				switch(type) // Switch on type
				{
					case ProgressType.LocalLoad:
						discs.localLoad.SetActive(true);
						break;
					case ProgressType.LocalSave:
						discs.localSave.SetActive(true);
						break;
					case ProgressType.CloudLoad:
						discs.cloudLoad.SetActive(true);
						break;
					case ProgressType.CloudSave:
						discs.cloudSave.SetActive(true);
						break;
				}
			}
		}

        // ------------------------------------------------------------------------------------
        // Local Reference Structures

        [Serializable]
		public class Discs
		{
			public GameObject localLoad;
			public GameObject localSave;
			public GameObject cloudLoad;
			public GameObject cloudSave;
		}
	}
}
