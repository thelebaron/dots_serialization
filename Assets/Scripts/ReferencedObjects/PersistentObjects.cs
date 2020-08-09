using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class PersistentObjects : ScriptableObject
{
    public Object Target;
    public List<EntityObject> Assets;
    
    [MenuItem("SerializeNew/Create PersistentObjects")]
    public static void CreateAsset()
    {
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<PersistentObjects>(), "Assets/PersistentObjects.asset");
    }
    
    /// <summary>
    /// Convert a referenced object to unique id
    /// </summary>
    [ContextMenu("ConvertTarget")]
    public void ConvertObject()
    {
        var entityObject = new EntityObject();
        
        var id = Target.GetHashCode() + Target.GetInstanceID();
        
        entityObject.referencedObject = Target;
        entityObject.id = id;
        entityObject.type = Target.GetType();
        Assets.Add(entityObject);
    }

    /// <summary>
    /// just log basic info about all items in the list
    /// </summary>
    [ContextMenu("Log")]
    public void LogAllEntry()
    {
        foreach (var asset in Assets)
        {
            Debug.Log("asset guid: " + asset.id + " | type: " + asset.type + " | referenced object: " +  asset.referencedObject);
        }
    }
}

[Serializable]
public class EntityObject
{
    public Object referencedObject;
    public Type type;
    public int id;
}

public struct SerialzedReference : IComponentData
{
    public int Id;
}