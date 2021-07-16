using PrefabSerialization;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PrefabId))]
[CanEditMultipleObjects]
public class SerializeableEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var script = target as PrefabId;

   
        /*if (GUILayout.Button("Generate Prefab Guid"))
        {
            script.GeneratePrefabGuid();
        }*/
        
        DrawDefaultInspector();
    }

}
#endif

