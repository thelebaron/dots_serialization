using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;
#if UNITY_EDITOR

[CustomEditor(typeof(PrefabDatabaseAuthoring))]
[CanEditMultipleObjects]
public class PrefabDatabaseAuthoringEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var script = target as PrefabDatabaseAuthoring;
   
        if (GUILayout.Button("Refresh Prefabs List"))
        {
            Refresh(script);
        }
        
        



        /*var duplicateComponents = script.gameObject.GetComponents<PrefabDatabaseAuthoring>();
        if (duplicateComponents.Length > 1)
        {
            EditorGUILayout.HelpBox("Warning, entity has multiple save components!", MessageType.Error);
        }
            
        
        EditorGUILayout.LabelField(script.guid, EditorStyles.boldLabel, GUILayout.MaxWidth(120));
        */
        DrawDefaultInspector();
    }

    private void Refresh(PrefabDatabaseAuthoring script)
    {
        var prefabs = SaveEntityUtility.GetAllPrefabs();
        DrawAllPrefabItems(prefabs, script);
    }
    
    
    private static void DrawAllPrefabItems(string[] prefabs, PrefabDatabaseAuthoring script)
    {
        script.Prefabs = new List<GameObject>();
        
        foreach (string path in prefabs)
        {
            //EditorGUILayout.BeginHorizontal();

            //var prefab = PrefabUtility.LoadPrefabContents(path);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            // should always have the component as we filter it in the GetAllPrefabs helper
            if (prefab.TryGetComponent<SaveEntityToDisk>(out var serializeable))
            {
                //Debug.Log(serializeable.guid);
                /*if (GUILayout.Button(path, GUILayout.MaxWidth(600)))
                {
                    //serializeable.guid = PrefabSerializeUtility.UniqueGuid();
                    //Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(path);
                }*/

                //EditorGUILayout.LabelField("", GUILayout.MaxWidth(60));
                //EditorGUILayout.LabelField("Prefab", GUILayout.MaxWidth(60));
                //EditorGUILayout.LabelField(serializeable.guid, EditorStyles.boldLabel, GUILayout.MaxWidth(80));
                
                script.Prefabs.Add(prefab);
            }

            EditorGUILayout.HelpBox("Added " + prefabs.Length + " prefabs.", MessageType.Info);
            //UnityEditor.PrefabUtility.UnloadPrefabContents(prefab);

            //EditorGUILayout.EndHorizontal();
        }
    }

}
#endif


