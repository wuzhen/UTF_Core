﻿using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // Common (Static)
    // - Common results data structures and returns
    // - Common conversion methods
    // - Common helper functions

    public class Common : MonoBehaviour
    {
        // Framework Information
        public static string applicationVersion = "1.0b1";

        // ------------------------------------------------------------------------------------
        // System

        // Get command line argument
        public static string GetArg(string name)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Getting command line arguments"); // Write to console
            var args = System.Environment.GetCommandLineArgs(); // Get all arguments
            for (int i = 0; i < args.Length; i++) // Iterate
            {
                if (args[i] == "-"+name && args.Length > i + 1) // If arg matches and has value
                {
                    return args[i + 1]; // Return value of arg
                }
            }
            return null; // Fail
        }

        // Quit application
        public static void QuitApplication()
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Quitting application"); // Write to console
            if (!Application.isEditor && Application.platform != RuntimePlatform.IPhonePlayer) // If not editor or iOS
                System.Diagnostics.Process.GetCurrentProcess().Kill(); // Kill process
            else if (Application.platform == RuntimePlatform.IPhonePlayer) // If iOS
                Application.Quit(); // Quit
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        // ------------------------------------------------------------------------------------
        // Shared data

        // Resolutions
        public static Dictionary<FrameResolution, Vector2> frameResolutionList = new Dictionary<FrameResolution, Vector2>
        {
            //{#, typeof(ExampleModel) }, // We dont include ExampleModel here as it is only for reference
            {FrameResolution.nHD , new Vector2(640, 360) },
            {FrameResolution.qHD , new Vector2(960, 540) },
            {FrameResolution.HD , new Vector2(1280, 720) },
            {FrameResolution.FullHD , new Vector2(1920, 1080) },
        };

        // Unity Versions
        public static string[] unityVersionList = new string[4]
        {
            "5.6",
            "2017.1",
            "2017.2",
            "2017.3"
        };

        // ------------------------------------------------------------------------------------
        // Get Common Data

        // Get common data about platform and version
        public static ResultsDataCommon GetCommonResultsData()
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Getting common results data"); // Write to console
            ResultsDataCommon output = new ResultsDataCommon(); // Create new class instance
			output.DateTime = Master.Instance.GetSystemTime ().ToString (); // Get SystemTime from Master
            SystemData systemData = Master.Instance.GetSystemData(); // Get SystemData from Master
            output.UnityVersion = systemData.UnityVersion; // Extract from SystemData
            output.AppVersion = systemData.AppVersion; // Extract from SystemData
            output.OS = systemData.OS; // Extract from SystemData
            output.Device = systemData.Device; // Extract from SystemData
            output.Platform = systemData.Platform; // Extract from SystemData
            output.API = systemData.API; // Extract from SystemData
			output.RenderPipe = Pipeline (); // Get the currently assigned pipeline
            output.Custom = ""; // Futureproof
            return output; // Return
        }        

		// Get Current Pipeline
		public static string Pipeline(){

			string defaultPipeline = "Standard Legacy"; // If no pipeline is loaded then will be set to this

			#if UNITY_5_6

			if(GraphicsSettings.renderPipelineAsset == null)
				return defaultPipeline; // return the default pipeline string
			else
				return GraphicsSettings.renderPipelineAsset.name; // Gets the currently active pieplines name in 5.6

			#elif UNITY_2017

			if(RenderPipelineManager.currentPipeline == null)
				return defaultPipeline; // return the default pipeline string
			else
				RenderPipelineManager.currentPipeline.name; // Gets the currently active pieplines name in 2017.1 TODO - not tested

			#endif

		}

        // ------------------------------------------------------------------------------------
        // Common Conversions

        // Convert string to dropdown option data
        public static UnityEngine.UI.Dropdown.OptionData ConvertStringToDropdownOptionData(string input)
        {
            UnityEngine.UI.Dropdown.OptionData newOption = new UnityEngine.UI.Dropdown.OptionData(); // Create new OptionData
            newOption.text = input; // Set text
            return newOption;
        }

        // Convert Array types ready to be serialized for saving
        public static string ConvertStringArrayToString(string[] input)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Converting String Array to String"); // Write to console
            StringBuilder stringBuilder = new StringBuilder(); // Create new StringBuilder
            foreach (string value in input) // Iterate input strings
            {
                stringBuilder.Append(value); // Append current input string
                stringBuilder.Append('|'); // Append character to split
            }
            return stringBuilder.ToString(); // Return
        }

        // Convert a Base64 String to a Texture2D
        // TODO - Remove commented code related to texture resolution, format and filtermode
        public static Texture2D ConvertStringToTexture(string textureName, /*byte[] input*/string input/*, Vector2 resolution, TextureFormat format, FilterMode filterMode*/)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Converting String to Texture2D"); // Write to console
            Texture2D output = new Texture2D(2, 2); // Create output Texture2D
            output.name = textureName; // Set texture name
            byte[] decodedBytes = Convert.FromBase64String(input); // Convert input string from Base64 to byte array
            output.LoadImage(decodedBytes); // Load image (PNG)
            return output; // Return
        }

        // Convert a RenderTexture to a Texture2D
        public static Texture2D ConvertRenderTextureToTexture2D(string textureName, RenderTexture input, Vector2 resolution, TextureFormat format, FilterMode filterMode)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Converting Render Texture to Texture2D"); // Write to console
            RenderTexture.active = input; // Set input as active RenderTexture
            Texture2D output = new Texture2D((int)resolution.x, (int)resolution.y, format, false); // Create new output Texture2D
            output.name = textureName; // Set texture name
            output.filterMode = filterMode; // Set filter mode
            output.ReadPixels(new Rect(0, 0, (int)resolution.x, (int)resolution.y), 0, 0); // Read pixels from active RenderTexture
            output.Apply(); // Apply
            RenderTexture.active = null; // Null active RenderTexture
            return output; // Return
        }

        // ------------------------------------------------------------------------------------
        // Helper functions

#if UNITY_EDITOR
        // Check if editor build target is standalone
        public static bool IsStandaloneTarget(UnityEditor.BuildTarget target)
        {
            bool output = false; // Create output
            foreach(UnityEditor.BuildTarget t in editorTargets) // Iterate editor target list
            {
                if (t == target) // If matches input target
                    output = true; // Set output to true
            }
            return output; // Return
        }

        // Editor target list (editor targets that are not emulated)
        public static UnityEditor.BuildTarget[] editorTargets = new UnityEditor.BuildTarget[8]
        {
            UnityEditor.BuildTarget.StandaloneLinux,
            UnityEditor.BuildTarget.StandaloneLinux64,
            UnityEditor.BuildTarget.StandaloneLinuxUniversal,
            UnityEditor.BuildTarget.StandaloneOSXIntel,
            UnityEditor.BuildTarget.StandaloneOSXIntel64,
            UnityEditor.BuildTarget.StandaloneOSXUniversal,
            UnityEditor.BuildTarget.StandaloneWindows,
            UnityEditor.BuildTarget.StandaloneWindows64
        };
#endif

        // Check if a test is applicable
        public static bool IsTestApplicable(Test input)
        {
            if (input.run == false) // If set to disabled
                return false; // Return false
            if (!IsCurrentPlatformInBitMask(input.platforms)) // If platform check fails
                return false; // Return false
            if (!IsUnityVersionAboveMinimum(input.minimumUnityVersion)) // If version check fails
                return false; // Return false
            return true; // All passed. Return true
        }

        // Find if current platform is selected within a platform bitmask
        public static bool IsCurrentPlatformInBitMask(int input)
        {
            int[] selectedPlatforms = GetPlatformSelectionFromBitMask(input); // Get selected platform indices from bitmask
            for (int i = 0; i < selectedPlatforms.Length; i++) // Iterate selected platforms
            {
                if (Enum.GetNames(typeof(RuntimePlatform))[selectedPlatforms[i]] == Application.platform.ToString()) // If index in full platform list matches current platform
                    return true; // Set to continue
            }
            return false; // Return false
        }

        // Find if Unity version is above specified index from unityVersionList
        public static bool IsUnityVersionAboveMinimum(int input)
        {
            Settings settings = SuiteManager.GetSettings(); // Get settings
            int versionIndex = 0; // Create version index
            for (int i = 0; i < unityVersionList.Length; i++) // Iterate version list
            {
                if (settings.unityVersion.Contains(unityVersionList[i])) // If unity version contains current index
                    versionIndex = i; // Set output index
            }
            if (input > versionIndex) // If minimum is higher than current
                return false; // Return false
            else
                return true; // Return true
        }

        // Get a platform selection array from bitmask
        public static int[] GetPlatformSelectionFromBitMask(int bitMask)
        {
            int length = Enum.GetNames(typeof(RuntimePlatform)).Length; // Get length of platform list
            List<int> intList = new List<int>(); // Create int list to track
            for (int i = 0; i < length; i++) // Iterate platform list
            {
                if (bitMask == (bitMask | (1 << i))) // If bit mask returns true
                {
                    intList.Add(i); // Add to list
                }
            }
            return intList.ToArray(); // Return list as array
        }

        public enum TimePeriod { Year, Month, Day, Hour, Minute, Second, Closest };

        // Compare two DateTimes and return the difference
        public static float GetTimeDifference(DateTime start, DateTime end, ref TimePeriod period)
        {
            float output = 0f; // Create output
            switch(period) // Switch on incoming time period
            {
                case TimePeriod.Year:
                    output = (float)(end - start).TotalDays / 365; // Return years
                    break;
                case TimePeriod.Month:
                    output = (float)(end - start).TotalDays / 31; // Return months (approx)
                    break;
                case TimePeriod.Day:
                    output = (float)(end - start).TotalDays; // Return days
                    break;
                case TimePeriod.Hour:
                    output = (float)(end - start).TotalHours; // Return hours
                    break;
                case TimePeriod.Minute:
                    output = (float)(end - start).TotalMinutes; // Return minutes
                    break;
                case TimePeriod.Second:
                    output = (float)(end - start).TotalSeconds; // Return seconds
                    break;
                case TimePeriod.Closest:
                    if ((end - start).TotalDays >= 365) // If over a year ago
                    {
                        output = Mathf.Floor((float)(end - start).TotalDays / 365); // Round years
                        period = TimePeriod.Year; // Set period
                    }
                    else if ((end - start).TotalDays >= 31) // If over a month ago (approx)
                    {
                        output = Mathf.Floor((float)(end - start).TotalDays / 31); // Round months (approx)
                        period = TimePeriod.Month; // Set period
                    }
                    else if ((end - start).TotalDays >= 1) // If over a day ago
                    {
                        output = Mathf.Floor((float)(end - start).TotalDays); // Return days
                        period = TimePeriod.Day; // Set period
                    }
                    else if ((end - start).TotalHours >= 1) // If over an hour ago
                    {
                        output = Mathf.Floor((float)(end - start).TotalHours); // Return hours
                        period = TimePeriod.Hour; // Set period
                    }
                    else if ((end - start).TotalMinutes >= 1) // If over a minute ago
                    {
                        output = Mathf.Floor((float)(end - start).TotalMinutes); // Return minutes
                        period = TimePeriod.Minute; // Set period
                    }
                    else
                    {
                        output = Mathf.Floor((float)(end - start).TotalSeconds); // Return seconds
                        period = TimePeriod.Second; // Set period
                    }  
                    break;
            }
            return output; // Return
        }

        // Prevent divide by zero
        static public float SafeDivision(float numerator, float denominator)
        {
            return (denominator == 0) ? 0 : numerator / denominator;
        }

        // Get a comparison value from a texture
        public static float GetTextureComparisonValue(Texture2D baselineInput, Texture2D resultsInput)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Getting Texture2D comparison value"); // Write to console
            float value = 0; // Create float for total pixel value
            int i = 0; // Create index for pixel count
            for (int x = 0; x < resultsInput.width; x++) // Iterate pixel columns
            {
                for (int y = 0; y < resultsInput.height; y++) // Iterate pixel rows
                {
                    Color c1 = baselineInput.GetPixel(x, y); // Get pixel from Baseline texture
                    Color c2 = resultsInput.GetPixel(x, y); // Get pixel from Results texture
                    float compR = Mathf.Abs(c2.r - c1.r); // Get comparison value from red channel
                    float compG = Mathf.Abs(c2.g - c1.g); // Get comparison value from green channel
                    float compB = Mathf.Abs(c2.b - c1.b); // Get comparison value from blue channel
                    value += ((compR + compG + compB) / 3); // Add average comparison value to total pixel value
                    i++; // Increment pixel count index
                }
            }
            return (value / i) * 100; // Divide total value by pixel count and multiply by 100 to return average percent
        }
    }

    // ------------------------------------------------------------------------------------
    // Global Data Structures

    [System.Serializable]
    public class ResultsDataCommon
    {
        public string DateTime;
        public string UnityVersion;
        public string AppVersion;
        public string OS;
        public string Device;
        public string Platform;
        public string API;
		public string RenderPipe;
        public string GroupName;
        public string TestName;
        public bool PassFail;
        public string Custom;
    }

    // ------------------------------------------------------------------------------------
    // Resolutions

    public enum FrameResolution
    {
        [Tooltip("640x360")]
        nHD,
        [Tooltip("960x540")]
        qHD,
        [Tooltip("1280x720")]
        HD,
        [Tooltip("1920x1080")]
        FullHD
    }
}
