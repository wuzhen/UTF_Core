using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace GraphicsTestFramework
{
	public enum ProgressType { LocalLoad, LocalSave, CloudLoad, CloudSave }

	public class ProgressScreen : MonoBehaviour 
	{
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

		public Discs discs;
		public Text messageText;
		public GameObject progressObject;
		public ProgressType progressType;

		public void SetState(bool active, ProgressType type, string message)
		{
			progressObject.SetActive(active);
			if(active == true)
			{
				discs.localLoad.SetActive(false);
				discs.localSave.SetActive(false);
				discs.cloudLoad.SetActive(false);
				discs.cloudSave.SetActive(false);

				progressType = type;
				messageText.text = message;

				switch(progressType)
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
