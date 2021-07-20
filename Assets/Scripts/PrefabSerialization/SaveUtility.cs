
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.Entities;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;


#if UNITY_EDITOR
public static class SaveUtility
{
    private const string kPrefabDatabase = "Prefab Database";
    
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


    /// <summary>
    /// Gets the scene PrefabDatabaseAuthoring component, and updates the list of prefabs.
    /// </summary>
    public static void ValidateSceneEntityDatabase()
    {
        var results = GameObject.FindObjectsOfType<PrefabDatabaseAuthoring>();

        // If zero results
        if (results.Length.Equals(0))
        {
            using (new EditorGUI.DisabledGroupScope(true))
            {
                EditorGUILayout.ToggleLeft(EditorGUIUtility.TrTempContent("Warning, no database entity gameobject found."), false);
            }
            
            if (GUILayout.Button("Create Prefab Database Entity", GUILayout.MaxWidth(250)))
            {
                var go = new GameObject();
                go.AddComponent<ConvertToEntity>();
                go.name = kPrefabDatabase;
                var component = go.AddComponent<PrefabDatabaseAuthoring>();
                component.Prefabs = SaveUtility.ReturnAllPrefabSaveEntities();
            }
        }
        
        // If zero results
        if (results.Length>1)
        {
            using (new EditorGUI.DisabledGroupScope(true))
            {
                EditorGUILayout.ToggleLeft(EditorGUIUtility.TrTempContent("Error, multiple database entity gameobjects found."), false);
            }
            if (GUILayout.Button("Prune variants", GUILayout.MaxWidth(250)))
            {
                for (int i = results.Length - 1; i >= 1; i--)
                {
                    Object.DestroyImmediate(results[i].gameObject);
                }
                results[0].gameObject.name = kPrefabDatabase;
            }
        }
        
        if (results.Length.Equals(1))
        {
            //EditorGUILayout.HelpBox("Found, database entity gameobject, entities can be serialized.", MessageType.None);
            using (new EditorGUI.DisabledGroupScope(true))
            {
                EditorGUILayout.ToggleLeft(EditorGUIUtility.TrTempContent("Database entity gameobject found, entities can be serialized."), true);
                EditorGUILayout.ObjectField(kPrefabDatabase, results[0].gameObject, typeof(GameObject), true);
            }

            var databaseAuthoring = results[0];
            
            if (databaseAuthoring.GetComponent<ConvertToEntity>() == null)
                databaseAuthoring.gameObject.AddComponent<ConvertToEntity>();

            
            // Enforce validity
            // ReSharper disable once ReplaceWithSingleAssignment.False
            bool reset = false;
            
            if (databaseAuthoring.Prefabs.Count != SaveUtility.ReturnAllPrefabSaveEntities().Count)
            {
                reset = true;
            }
            // Double check no missing/null
            for (int i = 0; i < databaseAuthoring.Prefabs.Count; i++)
            {
                if (databaseAuthoring.Prefabs[i] == null)
                {
                    reset = true;
                    break;
                }
            }
            if(reset)
                databaseAuthoring.Prefabs = SaveUtility.ReturnAllPrefabSaveEntities();
        }
    }
    

    public static List<GameObject> ReturnAllPrefabSaveEntities()
    {
        var prefabPaths = GetAllPrefabs();
        
        var list = new List<GameObject>();

        foreach (string path in prefabPaths)
        {
            //EditorGUILayout.BeginHorizontal();
            //var prefab = PrefabUtility.LoadPrefabContents(path);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            // should always have the component as we filter it in the GetAllPrefabs helper
            if (prefab.TryGetComponent<SaveEntityToDisk>(out var serializeable))
            {
                //script.Prefabs.Add(prefab);
                list.Add(prefab);
            }
        }

        return list;
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