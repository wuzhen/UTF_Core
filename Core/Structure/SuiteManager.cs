using System;
using System.Collections.Generic;
using UnityEngine;

namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // SuiteManager
    // - Collects Suite Scriptable Objects
    // - Builds local Suite and Scene structure for use by TestStructure

    public static class SuiteManager
    {
        // ------------------------------------------------------------------------------------
        // Get Data

        // Get a string array of all suite names
        public static string[] GetSuiteNames()
        {
            Settings settings = GetSettings(); // Get the suite list
            string[] suiteNames = new string[settings.suiteList.Count]; // Create string array of correct length
            for (int i = 0; i < suiteNames.Length; i++) // Iterate suites
                suiteNames[i] = settings.suiteList[i].suiteName; // Add to array
            return suiteNames; // Return
        }

        // Get a specific suite name
        public static string GetSuiteName(int index)
        {
            Settings settings = GetSettings(); // Get the suite list
            return settings.suiteList[index].suiteName; // Return requested
        }

        // Get a specific test
        public static Test GetTest(TestEntry inputEntry)
        {
            Settings settings = GetSettings(); // Get the suite list
            return settings.suiteList[inputEntry.suiteIndex].groups[inputEntry.groupIndex].tests[inputEntry.testIndex]; // Return requested
        }

        // Get the settings object
        public static Settings GetSettings()
        {
            Settings[] settingsArray = Resources.LoadAll<Settings>(""); // Find all suite lists
            if (settingsArray.Length == 0) // If no suite list found
            {
#if UNITY_EDITOR
                    return GenerateSettings(); // Create one
#else
                Console.Instance.Write(DebugLevel.Critical, MessageLevel.LogError, "No Suite List found. Aborting."); // Write to console
                return null;
#endif
            }
            else
                return settingsArray[0]; // Return suite list
        }

        // ------------------------------------------------------------------------------------
        // Editor Methods

#if UNITY_EDITOR

        // Create Suite and Scene structure
        [ExecuteInEditMode]
        public static void GenerateSceneList(bool debug)
        {
            Settings settings = GetSettings(); //Get the suite list
            settings.suiteList.Clear(); // Clear suites list
            Suite[] foundSuites = Resources.LoadAll<Suite>(""); // Load all Suite scriptable objects into array
            for(int i = 0; i < foundSuites.Length; i++)
            {
                if (debug && foundSuites[i].isDebugSuite || !debug && !foundSuites[i].isDebugSuite)
                    settings.suiteList.Add(foundSuites[i]);
            }
            UnityEditor.EditorUtility.SetDirty(settings); // Set dirty
            List<UnityEditor.EditorBuildSettingsScene> buildSettingsScenes = new List<UnityEditor.EditorBuildSettingsScene>(); // Create new build settings scene list
            AddManualMasterScene(buildSettingsScenes); // Add manual master TODO - Switch this for full automation
            for (int su = 0; su < settings.suiteList.Count; su++) // Iterate scriptable object list
            {
                for (int gr = 0; gr < settings.suiteList[su].groups.Count; gr++) // Iterate groups on the suite
                {
                    for (int te = 0; te < settings.suiteList[su].groups[gr].tests.Count; te++) // Iterate tests on the group
                    {
                        settings.suiteList[su].groups[gr].tests[te].scenePath = UnityEditor.AssetDatabase.GetAssetPath(settings.suiteList[su].groups[gr].tests[te].scene);
                        UnityEditor.EditorBuildSettingsScene scene = new UnityEditor.EditorBuildSettingsScene(settings.suiteList[su].groups[gr].tests[te].scenePath, true); // Create new build settings scene from asset path
                        if (!FindDuplicateScene(buildSettingsScenes, settings.suiteList[su].groups[gr].tests[te].scenePath)) // If no duplicate scene found
                            buildSettingsScenes.Add(scene); // Add to build settings scenes list
                    }
                }
            }
            UnityEditor.EditorBuildSettings.scenes = buildSettingsScenes.ToArray(); // Set build settings scene list
        }

        // Generate a new suite list object
        static Settings GenerateSettings()
        {
            Settings newSettings = ScriptableObject.CreateInstance<Settings>(); // Create instance
            if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/Resources")) // Check folder exists
                UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources"); // Create it
            UnityEditor.AssetDatabase.CreateAsset(newSettings, "Assets/Resources/Settings.asset"); // Create asset
            UnityEditor.AssetDatabase.SaveAssets(); // Save assets
            UnityEditor.AssetDatabase.Refresh(); // Refresh database
            return newSettings; // Return the suite list
        }

        // Add the manual master scene
        static void AddManualMasterScene(List<UnityEditor.EditorBuildSettingsScene> buildSettingsScenes)
        {
            string[] foundAssets = UnityEditor.AssetDatabase.FindAssets("Master t:Scene"); // Find master scene
            string masterScenePath = UnityEditor.AssetDatabase.GUIDToAssetPath(foundAssets[0]); // Get scene path for Master scene
            buildSettingsScenes.Add(new UnityEditor.EditorBuildSettingsScene(masterScenePath, true)); // Add to build settings scene list
        }

        // Find duplicate suite by name
        static bool FindDuplicateSuite(string name)
        {
            Settings settings = GetSettings(); // Get the suite list
            foreach (Suite suite in settings.suiteList) // Iterate local suites
            {
                if (suite.suiteName == name) // If equal to input suite
                    return true; // Duplicate. Return true
            }
            return false; // No duplicate. Return false
        }

        // Find duplicate scene in build settings by asset path
        static bool FindDuplicateScene(List<UnityEditor.EditorBuildSettingsScene> buildSettingsScenes, string path)
        {
            foreach (UnityEditor.EditorBuildSettingsScene edScene in buildSettingsScenes) // Iterate build settings scenes
            {
                if (edScene.path == path) // If equal to asset path
                    return true; // Duplicate. Return true
            }
            return false; // No duplicate. Return false
        }
#endif
    }
}
