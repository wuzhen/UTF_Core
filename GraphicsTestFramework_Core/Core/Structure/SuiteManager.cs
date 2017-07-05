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
        // Variables

        // Singleton
        /*private static SuiteManager _Instance = null;
        public static SuiteManager Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = (SuiteManager)FindObjectOfType(typeof(SuiteManager));
                return _Instance;
            }
        }*/

        // Data
        //public List<Suite> suites = new List<Suite>(); // Suite list
        

        // ------------------------------------------------------------------------------------
        // Get Data

        // Get a string array of all suite names
        public static string[] GetSuiteNames()
        {
            //Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Getting suite names"); // Write to console
            SuiteList suiteList = GetSuiteList(); // Get the suite list
            
            string[] suiteNames = new string[suiteList.suites.Count]; // Create string array of correct length
            for (int i = 0; i < suiteNames.Length; i++) // Iterate suites
                suiteNames[i] = suiteList.suites[i].suiteName; // Add to array
            return suiteNames; // Return
        }

        // Get a specific suite name
        public static string GetSuiteName(int index)
        {
            //Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Getting suite name"); // Write to console
            SuiteList suiteList = GetSuiteList(); // Get the suite list
            return suiteList.suites[index].suiteName; // Return requested
        }

        // Get a specific test
        public static Test GetTest(TestEntry inputEntry)
        {
            SuiteList suiteList = GetSuiteList(); // Get the suite list
            return suiteList.suites[inputEntry.suiteIndex].groups[inputEntry.groupIndex].tests[inputEntry.testIndex]; // Return requested
        }

        // Get the suite list object
        public static SuiteList GetSuiteList()
        {
            SuiteList[] suiteListArray = Resources.LoadAll<SuiteList>(""); // Find all suite lists
            if (suiteListArray.Length == 0) // If no suite list found
            {
#if UNITY_EDITOR
                    return GenerateSuiteList(); // Create one
#else
                    Console.Instance.Write(DebugLevel.Critical, MessageLevel.LogError, "No Suite List found. Aborting."); // Write to console
                    return null;
#endif
            }
            else
                return suiteListArray[0]; // Return suite list
        }

        // ------------------------------------------------------------------------------------
        // Editor Methods

#if UNITY_EDITOR

        // Setup for build
        [UnityEditor.MenuItem("RuntimeTestFramework/Prepare Build")]
        public static void PrepareBuild()
        {
            GenerateSceneList(false); // Create suite structure
            int platformCount = Enum.GetNames(typeof(UnityEditor.BuildTargetGroup)).Length; // Get platform count
            for (int i = 0; i < platformCount; i++) // Iterate all platforms
                UnityEditor.PlayerSettings.SetApplicationIdentifier((UnityEditor.BuildTargetGroup)i, "com.UnityTechnologies.RuntimeTestFramework"); // Set bundle identifiers
            UnityEditor.PlayerSettings.bundleVersion = Common.applicationVersion; // Set application version
        }

        // Setup for debug build
        [UnityEditor.MenuItem("RuntimeTestFramework/Prepare Debug Build")]
        public static void PrepareDebugBuild()
        {
            GenerateSceneList(true); // Create suite structure
            int platformCount = Enum.GetNames(typeof(UnityEditor.BuildTargetGroup)).Length; // Get platform count
            for (int i = 0; i < platformCount; i++) // Iterate all platforms
                UnityEditor.PlayerSettings.SetApplicationIdentifier((UnityEditor.BuildTargetGroup)i, "com.UnityTechnologies.RuntimeTestFramework"); // Set bundle identifiers
            UnityEditor.PlayerSettings.bundleVersion = Common.applicationVersion; // Set application version
        }

        // Create Suite and Scene structure
        [ExecuteInEditMode]
        public static void GenerateSceneList(bool debug)
        {
            //Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Creating Suite and Scene structure"); // Write to console
            SuiteList suiteList = GetSuiteList(); //Get the suite list
            suiteList.suites.Clear(); // Clear suites list
            Suite[] foundSuites = Resources.LoadAll<Suite>(""); // Load all Suite scriptable objects into array
            for(int i = 0; i < foundSuites.Length; i++)
            {
                if (debug && foundSuites[i].isDebugSuite || !debug && !foundSuites[i].isDebugSuite)
                    suiteList.suites.Add(foundSuites[i]);
            }
            UnityEditor.EditorUtility.SetDirty(suiteList); // Set dirty
            List<UnityEditor.EditorBuildSettingsScene> buildSettingsScenes = new List<UnityEditor.EditorBuildSettingsScene>(); // Create new build settings scene list
            AddManualMasterScene(buildSettingsScenes); // Add manual master TODO - Switch this for full automation
            for (int su = 0; su < suiteList.suites.Count; su++) // Iterate scriptable object list
            {
                for (int gr = 0; gr < suiteList.suites[su].groups.Count; gr++) // Iterate groups on the suite
                {
                    for (int te = 0; te < suiteList.suites[su].groups[gr].tests.Count; te++) // Iterate tests on the group
                    {
                        UnityEditor.EditorBuildSettingsScene scene = new UnityEditor.EditorBuildSettingsScene(suiteList.suites[su].groups[gr].tests[te].scenePath, true); // Create new build settings scene from asset path
                        if (!FindDuplicateScene(buildSettingsScenes, suiteList.suites[su].groups[gr].tests[te].scenePath)) // If no duplicate scene found
                            buildSettingsScenes.Add(scene); // Add to build settings scenes list
                    }
                }
            }
            UnityEditor.EditorBuildSettings.scenes = buildSettingsScenes.ToArray(); // Set build settings scene list
        }

        // Generate a new suite list object
        static SuiteList GenerateSuiteList()
        {
            SuiteList newSuiteList = ScriptableObject.CreateInstance<SuiteList>(); // Create instance
            if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/Resources")) // Check folder exists
                UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources"); // Create it
            UnityEditor.AssetDatabase.CreateAsset(newSuiteList, "Assets/Resources/SuiteList.asset"); // Create asset
            UnityEditor.AssetDatabase.SaveAssets(); // Save assets
            UnityEditor.AssetDatabase.Refresh(); // Refresh database
            return newSuiteList; // Return the suite list
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
            //Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Checking for duplicate suite"); // Write to console
            SuiteList suiteList = GetSuiteList(); // Get the suite list
            foreach (Suite suite in suiteList.suites) // Iterate local suites
            {
                if (suite.suiteName == name) // If equal to input suite
                    return true; // Duplicate. Return true
            }
            return false; // No duplicate. Return false
        }

        // Find duplicate scene in build settings by asset path
        static bool FindDuplicateScene(List<UnityEditor.EditorBuildSettingsScene> buildSettingsScenes, string path)
        {
            //Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Checking for duplicate scene"); // Write to console
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
