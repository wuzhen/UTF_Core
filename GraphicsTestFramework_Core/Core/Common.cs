using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

namespace GraphicsTestFramework
{
    public class Common : MonoBehaviour
    {
        //Get common data about platform and version
        public static ResultsDataCommon GetCommonResultsData()
        {
            ResultsDataCommon output = new ResultsDataCommon();
			output.DateTime = Master.Instance.GetSystemTime ().ToString ();
            SystemData systemData = Master.Instance.GetSystemData();
            output.UnityVersion = systemData.UnityVersion;
            output.AppVersion = systemData.AppVersion;
            output.Platform = systemData.Platform;
            output.API = systemData.API;
			output.RenderPipe = "Standard Legacy";
            return output; 
        }

        //Gets all subtypes of a Type
        public static List<Type> GetSubTypes<T>() where T : class
        {
            var types = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName.StartsWith("Mono.Cecil"))
                    continue;
                if (assembly.FullName.StartsWith("UnityScript"))
                    continue;
                if (assembly.FullName.StartsWith("Boo.Lan"))
                    continue;
                if (assembly.FullName.StartsWith("System"))
                    continue;
                if (assembly.FullName.StartsWith("I18N"))
                    continue;
                if (assembly.FullName.StartsWith("UnityEngine"))
                    continue;
                if (assembly.FullName.StartsWith("UnityEditor"))
                    continue;
                if (assembly.FullName.StartsWith("mscorlib"))
                    continue;
                foreach (Type type in assembly.GetTypes())
                {
                    if (!type.IsClass)
                        continue;
                    if (type.IsAbstract)
                        continue;
                    if (!type.IsSubclassOf(typeof(T)))
                        continue;
                    types.Add(type);
                }
            }
            return types;
        }

        // Used to convert array types ready to be serialized for saving
        public static string ConvertStringArrayToString(string[] input)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string value in input)
            {
                stringBuilder.Append(value);
                stringBuilder.Append('|');
            }
            return stringBuilder.ToString();
        }

        //Builds a Texture2D from a byte array
        public static Texture2D BuildTextureFromByteArray(string textureName, /*byte[] input*/string input, Vector2 resolution, TextureFormat format, FilterMode filterMode)
        {
            Texture2D output = new Texture2D((int)resolution.x, (int)resolution.y, format, false);
            output.name = textureName;
            output.filterMode = filterMode;
            Debug.LogWarning("res = " + resolution.x + ":" + resolution.y + " image length = " + input.Length);
            Debug.LogWarning(input);
            byte[] decodedBytes = Convert.FromBase64String(input);
            output.LoadImage(decodedBytes);
            //output.LoadImage(input);
            return output;
        }

        //Converts a RenderTexture to a Texture2D
        public static Texture2D ConvertRenderTextureToTexture2D(string textureName, RenderTexture input, Vector2 resolution, TextureFormat format, FilterMode filterMode)
        {
            RenderTexture.active = input;
            Texture2D output = new Texture2D((int)resolution.x, (int)resolution.y, format, false);
            output.name = textureName;
            output.filterMode = filterMode;
            output.ReadPixels(new Rect(0, 0, (int)resolution.x, (int)resolution.y), 0, 0);
            output.Apply();
            RenderTexture.active = null;
            return output;
        }

        //Gets a comparison value from a texture
        /*public static float GetTextureComparisonValue(Texture2D input)
        {
            float value = 0;
            int i = 0;
            for (int x = 0; x < input.width; x++)
            {
                for (int y = 0; y < input.height; y++)
                {
                    Color c = input.GetPixel(x, y);
                    value += ((c.r + c.g + c.b) / 3);
                    i++;
                }
            }
            return (value / i) * 100;
        }*/

        //Gets a comparison value from a texture
        public static float GetTextureComparisonValue(Texture2D baselineInput, Texture2D resultsInput)
        {
            float value = 0;
            int i = 0;
            for (int x = 0; x < resultsInput.width; x++)
            {
                for (int y = 0; y < resultsInput.height; y++)
                {
                    Color c1 = baselineInput.GetPixel(x, y);
                    Color c2 = resultsInput.GetPixel(x, y);
                    float compR = Mathf.Abs(c2.r - c1.r);
                    float compG = Mathf.Abs(c2.g - c1.g);
                    float compB = Mathf.Abs(c2.b - c1.b);
                    //Color comp = new Color(compR, compG, compB);
                    value += ((compR + compG + compB) / 3);
                    i++;
                }
            }
            return (value / i) * 100;
        }
    }

    [System.Serializable]
    public class ResultsDataCommon
    {
        public string DateTime; //Dont remove
        public string UnityVersion; //Dont remove
        public string AppVersion; //Dont remove
        public string Platform; //Dont remove
        public string API; //Dont remove
		public string RenderPipe; //Dont remove
        public string SceneName; //Dont remove
        public string TestName; //Dont remove
        public bool PassFail; //Dont remove
    }
}
