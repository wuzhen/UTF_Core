using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreUtilities
{
	
	public static class StringToUI
	{

		public static string Convert (string input)
		{
			char[] array = input.ToCharArray ();
			string output = "";
			for (int i = 0; i < array.Length; i++) {
				output += array [i];
			}
			return output;
		}
	}
}
