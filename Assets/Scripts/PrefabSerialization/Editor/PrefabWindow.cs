using UnityEngine;
using UnityEditor;
using System.Collections;
using PrefabSerialization;

public class PrefabWindow : EditorWindow

{
    string myString = "Hello World";
    bool   groupEnabled;
    bool   myBool  = true;
    float  myFloat = 1.23f;
    
    // Add menu item named "My Window" to the Window menu
    [MenuItem ("Prefabs/Window")]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        EditorWindow.GetWindow(typeof(PrefabWindow));
    }

    private void OnGUI()
    {
        GUILayout.Label ("Base Settings", EditorStyles.boldLabel);
        myString = EditorGUILayout.TextField ("Text Field", myString);
        
        groupEnabled = EditorGUILayout.BeginToggleGroup ("Optional Settings", groupEnabled);
        myBool       = EditorGUILayout.Toggle ("Toggle", myBool);
        myFloat      = EditorGUILayout.Slider ("Slider", myFloat, -3, 3);
        EditorGUILayout.EndToggleGroup ();


        var prefabs = PrefabSerializeUtility.GetAllPrefabs();
        
        foreach (string path in prefabs)
        {
            EditorGUILayout.BeginHorizontal();
            
            var prefab =  PrefabUtility.LoadPrefabContents(path);
            
            //Debug.Log(prefab);
            // should always have the component as we filter it in the GetAllPrefabs helper
            if (prefab.TryGetComponent<PrefabId>(out var serializeable))
            {
                //Debug.Log(serializeable.guid);
                if (GUILayout.Button(path, GUILayout.MaxWidth(600)))
                {
                    //serializeable.guid = PrefabSerializeUtility.UniqueGuid();
                }
                
                EditorGUILayout.LabelField("", GUILayout.MaxWidth(60));
                EditorGUILayout.LabelField("Prefab", GUILayout.MaxWidth(60));
                EditorGUILayout.LabelField(serializeable.guid, EditorStyles.boldLabel, GUILayout.MaxWidth(80));
            }


            
            UnityEditor.PrefabUtility.UnloadPrefabContents(prefab);
            
            EditorGUILayout.EndHorizontal();

        }
    }
}