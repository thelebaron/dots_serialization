using PrefabSerialization;
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
        EditorGUILayout.LabelField(script.guid, EditorStyles.boldLabel, GUILayout.MaxWidth(120));
        
        //DrawDefaultInspector();
    }

}
#endif

