using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphicsTestFramework
{
	public class TemporaryPullScript : MonoBehaviour {

		public ResultsDataCommon commonDataToPull;

		// Use this for initialization
		void Start () {
			
		}
		
		// Update is called once per frame
		void Update () {
			if(Input.GetKeyDown (KeyCode.Space)){
				ResultsIO.Instance.RetrieveResults ("PostProcessing", "Frame Comparison");
			}

			if(Input.GetKeyUp (KeyCode.Delete)){
				CloudConnectorCore.DeleteTable ("PostProcessing_Frame Comparison_Results", true);
			}
				
		}
	}
}
