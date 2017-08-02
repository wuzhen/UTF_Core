using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using System;

namespace GraphicsTestFramework
{
    public class BuildSettings : EditorWindow
    {
        // Scripting defines for the core
        static string[] coreScriptingDefines = new string[1] 
        {
            "UTF_EXISTS"
        };

        // Menu Item
        [MenuItem("RuntimeTestFramework/BuildSettings")]
        public static void ShowWindow()
        {
            GetWindow(typeof(BuildSettings)); // Get window
        }

        // GUI
        void OnGUI()
        {
            GUILayout.Label("Update Suite Information", EditorStyles.boldLabel); // Label
            if (GUILayout.Button("Build Suite List")) // If button
                PrepareBuild(); // Prepare build
            if (GUILayout.Button("Build Debug Suite List")) // If button
                PrepareDebugBuild(); // Prepare debug build
        }

        // Setup for build
        public static void PrepareBuild()
        {
            GetUnityVersionInfo(); // Get unity version info
            SuiteManager.GenerateSceneList(false); // Create suite structure
            int platformCount = Enum.GetNames(typeof(BuildTargetGroup)).Length; // Get platform count
            for (int i = 0; i < platformCount; i++) // Iterate all platforms
                PlayerSettings.SetApplicationIdentifier((BuildTargetGroup)i, "com.UnityTechnologies.RuntimeTestFramework"); // Set bundle identifiers
            SetScriptingDefines(); // Set defines
            SetPlayerSettings(); // Set player settings
            PlayerSettings.bundleVersion = Common.applicationVersion; // Set application version
        }

        // Setup for debug build
        public static void PrepareDebugBuild()
        {
            GetUnityVersionInfo(); // Get unity version info
            SuiteManager.GenerateSceneList(true); // Create suite structure
            int platformCount = Enum.GetNames(typeof(BuildTargetGroup)).Length; // Get platform count
            for (int i = 0; i < platformCount; i++) // Iterate all platforms
                PlayerSettings.SetApplicationIdentifier((BuildTargetGroup)i, "com.UnityTechnologies.RuntimeTestFramework"); // Set bundle identifiers
            SetScriptingDefines(); // Set defines
            SetPlayerSettings(); // Set player settings
            PlayerSettings.bundleVersion = Common.applicationVersion; // Set application version
        }

        public static void GetUnityVersionInfo()
        {
            Settings settings = SuiteManager.GetSettings(); // Get settings
            settings.unityVersion = UnityEditorInternal.InternalEditorUtility.GetFullUnityVersion(); // Set unity version
            settings.unityBranch = UnityEditorInternal.InternalEditorUtility.GetUnityBuildBranch(); // Set unity branch
        }

        // Set scripting define symbols
        static void SetScriptingDefines()
        {
            Settings settings = SuiteManager.GetSettings(); // Get settings
            string output = ""; // Create output string
            for (int i = 0; i < coreScriptingDefines.Length; i++) // Iterate core defines
                output += coreScriptingDefines[i] + ";"; // Add
            if (settings.scriptingDefines != null) // Check for null
            {
                for (int i = 0; i < settings.scriptingDefines.Length; i++) // Iterate settings defines
                    output += settings.scriptingDefines[i] + ";"; // Add
            }
            int platformCount = Enum.GetNames(typeof(BuildTargetGroup)).Length; // Get platform count
            for (int i = 0; i < platformCount; i++) // Iterate all platforms
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup((BuildTargetGroup)i, output); // Add custom to current
            }
        }

        // Set player settings
        static void SetPlayerSettings()
        {
            PlayerSettings.gpuSkinning = true;
            PlayerSettings.colorSpace = ColorSpace.Linear;
        }
    }

    // Build preprocess steps
    class MyCustomBuildProcessor : IPreprocessBuild
    {
        public int callbackOrder { get { return 0; } }

        public void OnPreprocessBuild(BuildTarget target, string path)
        {
#if UNITY_EDITOR
            BuildSettings.GetUnityVersionInfo();
#endif
        }
    }
}
