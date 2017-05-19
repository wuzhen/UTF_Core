using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace GraphicsTestFramework
{
    public class Build
    {
        [MenuItem("Graphics Test Framework/Build/Windows64")]
        public static void BuildWindows()
        {
            BuildClient(BuildTarget.StandaloneWindows64, ".exe");
        }

        [MenuItem("Graphics Test Framework/Build/OSX")]
        public static void BuildOSX()
        {
            BuildClient(BuildTarget.StandaloneOSXUniversal, ".app");
        }

        [MenuItem("Graphics Test Framework/Build/iOS")]
        public static void BuildiOS()
        {
            BuildClient(BuildTarget.iOS, ".app");
        }

        [MenuItem("Graphics Test Framework/Build/Android")]
        public static void BuildAndroid()
        {
            BuildClient(BuildTarget.Android, ".api");
        }

        public static void BuildClient(BuildTarget target, string extension)
        {
            string directory = UnityEngine.Application.dataPath.Replace("/Assets", "");
            if (!System.IO.Directory.Exists(directory + "/" + Master.Instance.buildDirectory))
                System.IO.Directory.CreateDirectory(directory + "/" + Master.Instance.buildDirectory);
            SuiteManager.Instance.BuildSuiteAndSceneLists();
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                UnityEngine.Debug.Log("Building Player, Directory: " + directory + "/"+Master.Instance.buildDirectory+"/" + Master.Instance.buildName + extension);
            try
            {
                PlayerSettings.companyName = "Unity Technologies";
                PlayerSettings.productName = "Graphics Test Framework";
                string build = BuildPipeline.BuildPlayer(GetBuildSettings(target, directory + "/" + Master.Instance.buildDirectory + "/" + Master.Instance.buildName + extension));
                UnityEngine.Debug.Log(build);
                if (Master.Instance.debugMode == Master.DebugMode.Messages)
                {
                    if(build != "")
                        UnityEngine.Debug.Log("Built Player, Directory: " + directory + "/" + Master.Instance.buildDirectory + "/" + Master.Instance.buildName + extension);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
        }

        static BuildPlayerOptions GetBuildSettings(BuildTarget target, string path)
        {
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            List<string> paths = new List<string>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
                paths.Add(scene.path);
            string[] scenes = paths.ToArray();
            buildPlayerOptions.scenes = scenes;
            buildPlayerOptions.locationPathName = path;
            buildPlayerOptions.target = target;
            buildPlayerOptions.options = BuildOptions.None;
            return buildPlayerOptions;
        }
    }
}
