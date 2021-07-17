using UnityEngine;
using UnityEditor;

public class SaveEntityWindow : EditorWindow
{
    // todo add version json to log warning if ids are recreated
    /*string myString = "Hello World";
    bool   groupEnabled;
    bool   myBool  = true;
    float  myFloat = 1.23f;*/
    
    // Add menu item named "My Window" to the Window menu
    [MenuItem ("DOTS/Save Entity Window")]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        EditorWindow.GetWindow(typeof(SaveEntityWindow));
    }

    private void OnGUI()
    {
        /*GUILayout.Label ("Base Settings for all Serializeable Entities", EditorStyles.boldLabel);
        myString = EditorGUILayout.TextField ("Text Field", myString);
        
        groupEnabled = EditorGUILayout.BeginToggleGroup ("Optional Settings", groupEnabled);
        myBool       = EditorGUILayout.Toggle ("Toggle", myBool);
        myFloat      = EditorGUILayout.Slider ("Slider", myFloat, -3, 3);
        EditorGUILayout.EndToggleGroup ();*/

        GUILayout.Space(15);
        GUILayout.Label ("Overview for all Save Entities(Prefabs)", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(15);
        
        GUILayout.Label ("Entities with a SaveToDisk component are shown here, as well as their corresponding id",GUILayout.Width(500));
        //GUILayout.FlexibleSpace();
        GUILayout.Space(15);
        EditorGUILayout.EndHorizontal();
        
        var prefabs = PrefabSerializeUtility.GetAllPrefabs();
        
        if (GUILayout.Button("Check for unique id collisions", GUILayout.MaxWidth(250)))
        {
            CheckForIdCollisions(prefabs);
        }
        
        DrawAllPrefabItems(prefabs);
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