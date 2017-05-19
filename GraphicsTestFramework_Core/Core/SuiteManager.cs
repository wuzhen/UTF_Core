using System;
using System.Collections.Generic;
using UnityEngine;

namespace GraphicsTestFramework
{
    public class SuiteManager : MonoBehaviour
    {
        [HideInInspector] public List<Suite> suites = new List<Suite>();

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

        /// <summary>
        /// EDITOR SIDE
        /// </summary>

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Graphics Test Framework/Get Scene List")]
        public static void GetSceneListMenu()
        {
            Instance.BuildSuiteAndSceneLists();
        }

        [ExecuteInEditMode]
        public void BuildSuiteAndSceneLists()
        {
            suites.Clear();
            GraphicsTestFramework.Suite[] allSuites = Resources.LoadAll<GraphicsTestFramework.Suite>("");
            List<UnityEditor.EditorBuildSettingsScene> buildSettingsScenes = new List<UnityEditor.EditorBuildSettingsScene>();
            string thisScenePath = this.gameObject.scene.path;
            buildSettingsScenes.Add(new UnityEditor.EditorBuildSettingsScene(thisScenePath, true));
            for (int i = 0; i < allSuites.Length; i++)
            {
                if (!FindDuplicateSuites(allSuites[i].name))
                {
                    Suite newSuite = new Suite();
                    newSuite.suiteName = allSuites[i].SuiteName;
                    for (int s = 0; s < allSuites[i].scenes.Length; s++)
                    {
                        if (allSuites[i].scenes[s] != null)
                        {
                            Scene newScene = new Scene();
                            string pathToScene = UnityEditor.AssetDatabase.GetAssetPath(allSuites[i].scenes[s]);
                            newScene.path = pathToScene;
                            newSuite.scenes.Add(newScene);
                            UnityEditor.EditorBuildSettingsScene scene = new UnityEditor.EditorBuildSettingsScene(pathToScene, true);
                            if (!FindDuplicateScenes(buildSettingsScenes, pathToScene))
                                buildSettingsScenes.Add(scene);
                        }
                    }
                    suites.Add(newSuite);
                }
            }
            UnityEditor.EditorBuildSettings.scenes = buildSettingsScenes.ToArray();
            for (int i = 0; i < suites.Count; i++)
            {
                for (int s = 0; s < suites[i].scenes.Count; s++)
                {
                    suites[i].scenes[s].scene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(suites[i].scenes[s].path);
                    suites[i].scenes[s].sceneName = suites[i].scenes[s].scene.name;
                }
            }
        }

        bool FindDuplicateSuites(string name)
        {
            foreach (Suite suite in suites)
            {
                if (suite.suiteName == name)
                    return true;
            }
            return false;
        }

        bool FindDuplicateScenes(List<UnityEditor.EditorBuildSettingsScene> buildSettingsScenes, string path)
        {
            foreach (UnityEditor.EditorBuildSettingsScene edScene in buildSettingsScenes)
            {
                if (edScene.path == path)
                    return true;
            }
            return false;
        }
#endif

		/// <summary>
		/// Fetches a string array of Suite names that are currently in the app
		/// </summary>
		public string[] GetSuiteNames(){
			string[] suiteNames = new string[suites.Count];

			for(int i = 0; i < suiteNames.Length; i++){
				suiteNames [i] = suites [i].suiteName;
			}

			return suiteNames;
		}


        /// <summary>
        /// DATA STRUCTURES
        /// </summary>

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
    }
}
