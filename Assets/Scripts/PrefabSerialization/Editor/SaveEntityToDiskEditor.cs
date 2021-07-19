using PrefabSerialization;
using UnityEditor.Experimental.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SaveEntityToDisk))]
[CanEditMultipleObjects]
public class SaveEntityToDiskEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var script = target as SaveEntityToDisk;
   
        /*if (GUILayout.Button("Generate Prefab Guid"))
        {
            script.GeneratePrefabGuid();
        }*/
        if (PrefabStageUtility.GetCurrentPrefabStage() == null)
        {
            if (!PrefabUtility.IsPartOfAnyPrefab(script.gameObject))
            {
                EditorGUILayout.HelpBox("Warning, GameObject is not part of any prefab!", MessageType.Error);
                return;
            }
        }

        var modifications = PrefabUtility.GetPropertyModifications(script);
        if (modifications != null)
        {
            foreach (var modification in modifications)
            {
                if (modification.target.name == "SaveEntityToDisk" && modification.propertyPath == "guid")
                {
                    EditorGUILayout.HelpBox("Warning, Save component has unapplied modifications!", MessageType.Warning);
                    return;
                }
            }
            

        }


        var duplicateComponents = script.gameObject.GetComponents<SaveEntityToDisk>();
        if (duplicateComponents.Length > 1)
        {
            EditorGUILayout.HelpBox("Warning, entity has multiple save components!", MessageType.Error);
        }
            
        
        EditorGUILayout.LabelField(script.guid, EditorStyles.boldLabel, GUILayout.MaxWidth(120));
        
        //DrawDefaultInspector();
    }

}
#endif

