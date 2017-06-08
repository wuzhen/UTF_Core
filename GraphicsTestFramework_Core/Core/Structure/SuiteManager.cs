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
        public Suite[] suites; // Suite list

        // ------------------------------------------------------------------------------------
        // Get Data

        // Get a string array of all suite names
        public string[] GetSuiteNames()
        {
            Console.Instance.Write(DebugLevel.Full, MessageLevel.Log, "Getting suite names"); // Write to console
            string[] suiteNames = new string[suites.Length]; // Create string array of correct length
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
            //suites.Clear(); // Clear current suite list
            suites = Resources.LoadAll<Suite>(""); // Load all Suite scriptable objects into array
            List<UnityEditor.EditorBuildSettingsScene> buildSettingsScenes = new List<UnityEditor.EditorBuildSettingsScene>(); // Create new build settings scene list
            AddManualMasterScene(buildSettingsScenes); // Add manual master TODO - Switch this for full automation
            for (int su = 0; su < suites.Length; su++) // Iterate scriptable object list
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
            /*for (int su = 0; su < allSuites.Length; su++) // Iterate scriptable object list
            {
                if (!FindDuplicateSuite(allSuites[i].name)) // If no duplicate suite found
                {
                    Suite newSuite = new Suite(); // Create new suite instance
                    newSuite.suiteName = allSuites[i].SuiteName; // Set name from scriptable object
                    for (int s = 0; s < allSuites[i].scenes.Length; s++) // Iterate scenes on the scriptable object
                    {
                        if (allSuites[i].scenes[s] != null) // If the scene reference isnt null
                        {
                            Scene newScene = new Scene(); // Create new scene instance
                            string pathToScene = UnityEditor.AssetDatabase.GetAssetPath(allSuites[i].scenes[s]); // Get scenes asset path
                            newScene.path = pathToScene; // Set asset path on scene instance
                            newSuite.scenes.Add(newScene); // Add scene instance to suites instances scene list
                            UnityEditor.EditorBuildSettingsScene scene = new UnityEditor.EditorBuildSettingsScene(pathToScene, true); // Create new build settings scene from asset path
                            if (!FindDuplicateScene(buildSettingsScenes, pathToScene)) // If no duplicate scene found
                                buildSettingsScenes.Add(scene); // Add to build settings scenes list
                        }
                    }
                    suites.Add(newSuite); // Add suite instance to local suite list
                }
            }
            UnityEditor.EditorBuildSettings.scenes = buildSettingsScenes.ToArray(); // Set build settings scene list
            for (int i = 0; i < suites.Count; i++) // Iterate local suite list
            {
                for (int s = 0; s < suites[i].scenes.Count; s++) // Iterate scenes
                {
                    suites[i].scenes[s].scene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(suites[i].scenes[s].path); // Get scene reference by asset path
                    suites[i].scenes[s].sceneName = suites[i].scenes[s].scene.name; // Set scene name
                }
            }*/
        }

        void AddManualMasterScene(List<UnityEditor.EditorBuildSettingsScene> buildSettingsScenes)
        {
            string thisScenePath = this.gameObject.scene.path; // Get scene path for Master scene
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

        // ------------------------------------------------------------------------------------
        // Local Data Structures
        /*
        [Serializable]
        public class Suite
        {
            public string suiteName;
            public List<Scene> scenes = new List<Scene>();
        }

        [Serializable]
        public class Scene
        {
            public string sceneName;
            public UnityEngine.SceneManagement.Scene scene;
            public string path;
        }
        */
    }
}
