using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace thelebaron.BuildTools
{
    public static class SemVer
    {
        [MenuItem("Version/Bump Patch")]
        public static void BumpPatch()
        {
            Bump(false, false, true);
        }
        [MenuItem("Version/Bump Minor")]
        public static void BumpMinor()
        {
            Bump(false, true, false);
        }        
        [MenuItem("Version/Bump Major")]
        public static void BumpMajor()
        {
            Bump(true, false, false);
        }
        
        private static void Bump(bool major, bool minor, bool patch)
        {
            GetVersion(out var gameVersion, out var filepath);

            if(patch)
                gameVersion.Patch++;
            if(minor)
                gameVersion.Minor++;
            if(major)
                gameVersion.Major++;
            
            var jsondata = JsonUtility.ToJson(gameVersion, true);
            File.WriteAllText(filepath, jsondata);
            
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
            
            var v = gameVersion.Major + "." + gameVersion.Minor + "." + gameVersion.Patch;
            UpdateProjectVersion(v);
        }
        
        private static void GetVersion(out GameVersion gameVersionData, out string filepath)
        {
            filepath = Application.dataPath + "/GameVersion.json";
            if (!File.Exists(Application.dataPath + "/GameVersion.json"))
            {
                var gameVersion = new GameVersion();
                JsonUtility.ToJson(gameVersion, true);

                gameVersionData = gameVersion;
                filepath = Application.dataPath + "/GameVersion.json";
                return;
            }
            var json = File.ReadAllText(Application.dataPath + "/GameVersion.json");
            gameVersionData = JsonUtility.FromJson<GameVersion>(json);

        }

        private static void UpdateProjectVersion(string version)
        {
            PlayerSettings.bundleVersion = version;
        }
    }

    public static class Versioning
    {
        [MenuItem("Version/Show Version")]
        private static void LogVersion()
        {
            // This gets the Build Version from Git via the `git describe` command
            //PlayerSettings.bundleVersion = Git.BuildVersion;
            ClearConsole();
            Debug.Log(PlayerSettings.bundleVersion);
        }
        

        /// <summary>
        /// Clears the console
        /// </summary>
        [MenuItem ("Version/Clear Console %#c")] // CMD + SHIFT + C
        public static void ClearConsole()
        {
            var assembly = Assembly.GetAssembly(typeof(SceneView));
            var type     = assembly.GetType("UnityEditor.LogEntries");
            var method   = type.GetMethod("Clear");
            method.Invoke(new object(), null);
        }
    }

    [Serializable]
    public class GameVersion
    {
        public int Major;
        public int Minor;
        public int Patch;
    }
}