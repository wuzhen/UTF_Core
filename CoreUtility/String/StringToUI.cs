using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace CoreUtilities
{
	
	public static class StringToUI
	{
        public static string Convert (string input)
		{
			char[] array = input.ToCharArray ();
			string output = "";
			for (int i = 0; i < array.Length; i++)
            {
                if(char.IsLetter(array[i]) || char.IsDigit(array[i]))
                    output += (int)array[i];
			}
            return output;
        }
	}
}
