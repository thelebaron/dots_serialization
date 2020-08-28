using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;
using Hash128 = Unity.Entities.Hash128;
using UnityObject = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
using DOTS.Serialization.ReferencedObjects;
#endif

public class AssetMap : ScriptableObject
{
    public List<EntityObject> assets;
    
    
#if UNITY_EDITOR
    
    [MenuItem("SerializeNew/Create or Refresh AssetMap")]
    public static void CreateAsset()
    {
        AssetDatabase.CreateAsset(CreateInstance<AssetMap>(), "Assets/Resources/AssetMap.asset");
        var asset = AssetDatabase.LoadAssetAtPath<AssetMap>("Assets/Resources/AssetMap.asset");
        asset.Refresh();
    }
    
    [ContextMenu("GetEveryAsset")]
    public void Refresh()
    {
        this.assets ??= new List<EntityObject>();
        this.assets?.Clear();
        var assets = EditorAssetDatabaseUtility.GetEverything();

        foreach (var obj in assets)
        {
            var entityObject = new EntityObject(obj);
        
            this.assets.Add(entityObject);
        }

        Debug.Log("Refreshed Assetmap with " + assets.Count  + " assets.");
    }



#endif
}

[Serializable]
public class EntityObject
{
    public UnityObject referencedObject;
    
    public Hash128 hash;
    
    /*static unsafe Hash128 GenerateDefaultGuid()
    {
        var guid = System.Guid.NewGuid();
        var hash = new Unity.Entities.Hash128();
        hash = *(Unity.Entities.Hash128*)&guid;
        return hash;
    }*/

    public EntityObject(UnityObject obj)
    {
        referencedObject = obj;
        hash = GenerateGuid(obj);
    }
    
    static unsafe Hash128 GenerateGuid(UnityObject obj)
    {
        Guid guid;
        var input = obj.name + obj.GetType();
        using (MD5 md5 = MD5.Create())
        {
            byte[] bytehash = md5.ComputeHash(Encoding.Default.GetBytes(input));
            guid = new Guid(bytehash);
        }
        
        var hash = new Unity.Entities.Hash128();
        hash = *(Unity.Entities.Hash128*)&guid;
        return hash;
    }

}

