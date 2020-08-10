using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using Object = UnityEngine.Object;
using Unity.Scenes;
#if UNITY_EDITOR
using UnityEditor;
using DOTS.Serialization.ReferencedObjects;
#endif

public class PersistentObjects : ScriptableObject
{
    public Object Target;
    public ReferencedUnityObjects ReferencedUnityObjects;
    public List<EntityObject> Assets;

    public static ReferencedUnityObjects RemappedObjects()
    {
        var objRefs = UnityEngine. ScriptableObject. CreateInstance<ReferencedUnityObjects>();
        // find and compare with json
        return null;
    }
    
#if UNITY_EDITOR
    
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
        var assets = EditorAssetDatabaseUtility.GetEverything();

        foreach (var obj in assets)
        {
            var entityObject = new EntityObject();
            var id = obj.GetHashCode();
        
            entityObject.referencedObject = obj;
            entityObject.id               = id;
            Assets.Add(entityObject);
        }
    }
#endif
}

[Serializable]
public class EntityObject
{
    public Object referencedObject;
    public int id;
}

