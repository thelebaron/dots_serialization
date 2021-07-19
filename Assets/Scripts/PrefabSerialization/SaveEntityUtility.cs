
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;


#if UNITY_EDITOR
public static class SaveEntityUtility
{
    public static string UniqueGuid()
    {
        var dictionary = new Dictionary<string, string>();
        List<string> prefabs  = GetAllSerializeablePrefabs();
        
        foreach (string path in prefabs)
        {
            GameObject prefab =  UnityEditor.PrefabUtility.LoadPrefabContents(path);
            
            if (prefab.GetComponent<SaveEntityToDisk>()!=null)
            {
                var serializeable = prefab.GetComponent<SaveEntityToDisk>();
                if (!dictionary.ContainsKey(serializeable.guid))
                {
                    dictionary.Add(serializeable.guid, path);
                }
            }
            //https://docs.unity3d.com/ScriptReference/PrefabUtility.SaveAsPrefabAsset.html
            
            UnityEditor.PrefabUtility.UnloadPrefabContents(prefab);
        }
        
        

        var guid = GenerateGuidString();

        while (true)
        {
            if (!dictionary.ContainsKey(guid))
                break;
            guid = GenerateGuidString();
        }

        return guid;
    }


    private static string GenerateGuidString()
    {
        var g = new StringBuilder("123456789");

        for (int i = 0; i < 8; i++)
        {
            var randomSeed = UnityEngine.Random.Range(0, 12345678);
            g[i] = GetChar(randomSeed);
        }

        return g.ToString();
    }

    private static char GetChar(int seed)
    {
        //string        chars = "$%#@!*abcdefghijklmnopqrstuvwxyz1234567890?;:ABCDEFGHIJKLMNOPQRSTUVWXYZ^&";
        string        chars = "abcdefghijklmnopqrstuvwxyz1234567890";
        System.Random rand  = new System.Random(seed);
        int           num   = rand.Next(0, chars.Length - 1);
        return chars[num];
    }

    private static List<string> GetAllSerializeablePrefabs()
    {
        string[]     assetPaths = GetAllPrefabs();
        List<string> prefabs    = new List<string>();
        foreach (string path in assetPaths)
        {
            var prefab =  UnityEditor.PrefabUtility.LoadPrefabContents(path);
            // Only add serializeable prefabs to the list
            if (prefab.GetComponent<SaveEntityToDisk>()!=null)
            {
                prefabs.Add(path);
            }
            UnityEditor.PrefabUtility.UnloadPrefabContents(prefab);
        }
        Debug.ClearDeveloperConsole();
        return prefabs;
    }
    
    /// <summary>
    /// Returns all Prefabs in a project
    /// </summary>
    public static string[] GetAllPrefabs()
    {
        string[]     allAssetPaths   = AssetDatabase.GetAllAssetPaths();
        List<string> allPrefabs = new List<string>();
        foreach (string s in allAssetPaths)
        {
            // Ensure its a prefab and not inside the packages folder(checking certain package prefabs in entities gives annoying warnings that cannot be supressed)
            if (s.Contains(".prefab") && !s.Contains("Packages/"))
            {
                if(!allPrefabs.Contains(s))
                    allPrefabs.Add(s);
            }
        }
        return allPrefabs.ToArray();
    }


    public static bool IsSaved(GameObject selectedGameObject)
    {
        if(selectedGameObject == null)
            return false;
        
        var saveComponent = selectedGameObject.GetComponent<SaveEntityToDisk>();

        if (saveComponent != null) 
            return true;
        return false;
    }
}
#endif