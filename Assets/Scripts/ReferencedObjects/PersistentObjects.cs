
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.Entities;
using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using DOTS.Serialization.ReferencedObjects;
using Unity.Scenes;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine.UIElements;

public class PersistentObjects : ScriptableObject
{
    public Object Target;
    public ReferencedUnityObjects ReferencedUnityObjects;
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
        var i = 0;
        foreach (var asset in ReferencedUnityObjects.Array)
        {
            i++;
            // Get size kb
            long   size = 0;
            object o    = asset;
            using (Stream s = new MemoryStream()) {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(s, o);
                size = s.Length;
            }

            Debug.Log("asset name: " + asset.name + " | size: " + size);//+ " | hash: " +  asset.GetHashCode());
            
        }
    }
    
    [ContextMenu("GetEveryAsset")]
    public void GetEveryAsset()
    {
        Assets ??= new List<EntityObject>();
        Assets?.Clear();
        var assets = AssetDatabaseUtility.GetEverything();

        foreach (var obj in assets)
        {
            var entityObject = new EntityObject();
            var id = obj.GetHashCode();
        
            entityObject.referencedObject = obj;
            entityObject.id               = id;
            entityObject.type             = Target.GetType();
            Assets.Add(entityObject);
        }
    }

}
#endif

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

