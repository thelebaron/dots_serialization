using Unity.Entities;
using UnityEngine;
using UnityEditor;

public class PrefabEntityWindow : EditorWindow
{
    // todo add version json to log warning if ids are recreated
    /*string myString = "Hello World";
    bool   groupEnabled;
    bool   myBool  = true;
    float  myFloat = 1.23f;*/
    private GameObject prefabStoreGameObject;
    private const string kPrefabDatabase = "Prefab Database";

    // Add menu item named "My Window" to the Window menu
    [MenuItem ("Tools/Save Entity Window")]
    public static void ShowWindow()
    {
        var window = EditorWindow.GetWindow<PrefabEntityWindow>(title: "Prefab Entities");
        window.Show();
        
    }

    private void OnGUI()
    {
        /*GUILayout.Label ("Base Settings for all Serializeable Entities", EditorStyles.boldLabel);
        myString = EditorGUILayout.TextField ("Text Field", myString);
        
        groupEnabled = EditorGUILayout.BeginToggleGroup ("Optional Settings", groupEnabled);
        myBool       = EditorGUILayout.Toggle ("Toggle", myBool);
        myFloat      = EditorGUILayout.Slider ("Slider", myFloat, -3, 3);
        EditorGUILayout.EndToggleGroup ();*/
        minSize = new Vector2(850,300);
        GUILayout.Space(15);
        GUILayout.Label ("Overview for all Save Entities(Prefabs)", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(15);
        
        GUILayout.Label ("Entities with a SaveToDisk component are shown here, as well as their corresponding id",GUILayout.Width(500));
        //GUILayout.FlexibleSpace();
        GUILayout.Space(15);
        EditorGUILayout.EndHorizontal();
        
        var prefabs = SaveUtility.GetAllPrefabs();
        
        ValidateForSceneEntity(prefabs);
                

        
        if (GUILayout.Button("Check for unique id collisions", GUILayout.MaxWidth(250)))
        {
            CheckForIdCollisions(prefabs);
        }


        DrawAllPrefabItems(prefabs);
    }

    private void ValidateForSceneEntity(string[] prefabs)
    {
        var results = GameObject.FindObjectsOfType<PrefabDatabaseAuthoring>();

        // If zero results
        if (results.Length.Equals(0))
        {
            //EditorGUILayout.HelpBox("Warning, no database entity gameobject found. ", MessageType.Warning);
            
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
            //EditorGUILayout.HelpBox("Error, multiple database entity gameobjects found ", MessageType.Error);
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

            // Enforce validity
            if (databaseAuthoring.Prefabs.Count != SaveUtility.ReturnAllPrefabSaveEntities().Count)
            {
                databaseAuthoring.Prefabs = SaveUtility.ReturnAllPrefabSaveEntities();
            }

            if (databaseAuthoring.GetComponent<ConvertToEntity>() == null)
                databaseAuthoring.gameObject.AddComponent<ConvertToEntity>();
        }
    }

    private static void DrawAllPrefabItems(string[] prefabs)
    {
        foreach (string path in prefabs)
        {
            EditorGUILayout.BeginHorizontal();

            var prefab = PrefabUtility.LoadPrefabContents(path);

            // should always have the component as we filter it in the GetAllPrefabs helper
            if (prefab.TryGetComponent<SaveEntityToDisk>(out var serializeable))
            {
                //Debug.Log(serializeable.guid);
                if (GUILayout.Button(path, GUILayout.MaxWidth(600)))
                {
                    //serializeable.guid = PrefabSerializeUtility.UniqueGuid();
                    Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(path);
                }

                EditorGUILayout.LabelField("", GUILayout.MaxWidth(60));
                EditorGUILayout.LabelField("Prefab", GUILayout.MaxWidth(60));
                EditorGUILayout.LabelField(serializeable.guid, EditorStyles.boldLabel, GUILayout.MaxWidth(80));
            }

            UnityEditor.PrefabUtility.UnloadPrefabContents(prefab);

            EditorGUILayout.EndHorizontal();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private static void CheckForIdCollisions(string[] prefabs)
    {
        var dictionary = new System.Collections.Generic.Dictionary<string, string>();
        foreach (string assetPath in prefabs)
        {
            var prefab = PrefabUtility.LoadPrefabContents(assetPath);
            if (prefab.TryGetComponent<SaveEntityToDisk>(out var saveComponent))
            {
                // Check if prefab path exists in dictionary
                if (!dictionary.ContainsKey(assetPath))
                {
                    var guid = saveComponent.guid;
                    if (!dictionary.ContainsValue(saveComponent.guid))
                    {
                        dictionary.Add(assetPath, saveComponent.guid);
                    }
                    else
                    {
                        EditorGUILayout.LabelField(assetPath + ": guid " + saveComponent.guid + " exists! recreate the guid for this asset");
                        if (GUILayout.Button(assetPath, GUILayout.MaxWidth(600)))
                        {
                            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                        }
                    }
                }
            }

            UnityEditor.PrefabUtility.UnloadPrefabContents(prefab);
        }
    }
}