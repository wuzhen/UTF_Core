using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GraphicsTestFramework
{
	public class ListEntry : MonoBehaviour 
	{
		public Text line1;
		public Text line2;

		public void Setup(string input1, string input2)
		{
			line1.text = input1;
			line2.text = input2;
		}
	}
}
