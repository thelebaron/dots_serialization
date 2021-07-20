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
        
        script.Prefabs = SaveUtility.ReturnAllPrefabSaveEntities();
        EditorGUILayout.HelpBox("Added " + script.Prefabs.Count + " prefabs.", MessageType.Info);
    }


}
#endif


