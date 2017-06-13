using System;
using System.Collections.Generic;
using UnityEngine;

namespace GraphicsTestFramework
{
    // ------------------------------------------------------------------------------------
    // SuiteManager
    // - Collects Suite Scriptable Objects
    // - Builds local Suite and Scene structure for use by TestStructure

    public class SuiteManager : MonoBehaviour
    {
        // ------------------------------------------------------------------------------------
        // Variables

        // Singleton
        private static SuiteManager _Instance = null;
        public static SuiteManager Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = (SuiteManager)FindObjectOfType(typeof(SuiteManager));
                return _Instance;
            }
        }

        // Data
        public bool includeDebug = true; // Include debug suites?
        public List<Suite> suites = new List<Suite>(); // Suite list

        // ------------------------------------------------------------------------------------
        // Get Data

        // Get a string array of all suite names
        public string[] GetSuiteNames()
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Getting suite names"); // Write to console
            string[] suiteNames = new string[suites.Count]; // Create string array of correct length
            for (int i = 0; i < suiteNames.Length; i++) // Iterate suites
                suiteNames[i] = suites[i].suiteName; // Add to array
            return suiteNames; // Return
        }

        // ------------------------------------------------------------------------------------
        // Editor Methods

#if UNITY_EDITOR

        // Menu entry to create Suite and Scene structure
        [UnityEditor.MenuItem("Graphics Test Framework/Get Scene List")]
        public static void GetSceneListMenu()
        {
            Instance.GenerateSceneList(); // Create structure
        }

        // Create Suite and Scene structure
        [ExecuteInEditMode]
        public void GenerateSceneList()
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Creating Suite and Scene structure"); // Write to console
            suites.Clear(); // Clear suites list
            Suite[] foundSuites = Resources.LoadAll<Suite>(""); // Load all Suite scriptable objects into array
            for(int i = 0; i < foundSuites.Length; i++)
            {
                if (includeDebug || !foundSuites[i].isDebugSuite)
                    suites.Add(foundSuites[i]);
            }
            List<UnityEditor.EditorBuildSettingsScene> buildSettingsScenes = new List<UnityEditor.EditorBuildSettingsScene>(); // Create new build settings scene list
            AddManualMasterScene(buildSettingsScenes); // Add manual master TODO - Switch this for full automation
            for (int su = 0; su < suites.Count; su++) // Iterate scriptable object list
            {
                for (int gr = 0; gr < suites[su].groups.Count; gr++) // Iterate groups on the suite
                {
                    for (int te = 0; te < suites[su].groups[gr].tests.Count; te++) // Iterate tests on the group
                    {
                        string pathToScene = UnityEditor.AssetDatabase.GetAssetPath(suites[su].groups[gr].tests[te].scene); // Get scene path
                        UnityEditor.EditorBuildSettingsScene scene = new UnityEditor.EditorBuildSettingsScene(pathToScene, true); // Create new build settings scene from asset path
                        if (!FindDuplicateScene(buildSettingsScenes, pathToScene)) // If no duplicate scene found
                            buildSettingsScenes.Add(scene); // Add to build settings scenes list
                    }
                }
            }
            UnityEditor.EditorBuildSettings.scenes = buildSettingsScenes.ToArray(); // Set build settings scene list
        }

        // Add the manual master scene
        void AddManualMasterScene(List<UnityEditor.EditorBuildSettingsScene> buildSettingsScenes)
        {
            string thisScenePath = gameObject.scene.path; // Get scene path for Master scene
            buildSettingsScenes.Add(new UnityEditor.EditorBuildSettingsScene(thisScenePath, true)); // Add to build settings scene list
        }

        // Find duplicate suite by name
        bool FindDuplicateSuite(string name)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Checking for duplicate suite"); // Write to console
            foreach (Suite suite in suites) // Iterate local suites
            {
                if (suite.suiteName == name) // If equal to input suite
                    return true; // Duplicate. Return true
            }
            return false; // No duplicate. Return false
        }

        // Find duplicate scene in build settings by asset path
        bool FindDuplicateScene(List<UnityEditor.EditorBuildSettingsScene> buildSettingsScenes, string path)
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Checking for duplicate scene"); // Write to console
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
