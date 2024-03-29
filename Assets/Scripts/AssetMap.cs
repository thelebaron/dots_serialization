using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using Utility;
using Hash128 = Unity.Entities.Hash128;
using UnityObject = UnityEngine.Object;
#if UNITY_EDITOR
using System.Collections;
using UnityEditor;

#endif

/// <summary>
/// An object holder of unity reference objects and their respective hashes.
/// This object is used as a reference when creating a json assetmap to store their hashes and names, and then remapped on deserializing
/// to match hash to actual unity object. This must be created in the editor and updated manually as a project grows.
/// should add methods to auto update on either editor start/shutdown or project building
/// </summary>
public class AssetMap : ScriptableObject
{
    public List<EntityObject> assets;
    
    
#if UNITY_EDITOR
    
    [MenuItem("SerializeNew/Create or Refresh AssetMap")]
    public static void CreateAsset()
    {
        var assetMap = AssetDatabase.LoadAssetAtPath<AssetMap>("Assets/Resources/AssetMap.asset");
        if(assetMap == null)
            AssetDatabase.CreateAsset(CreateInstance<AssetMap>(), "Assets/Resources/AssetMap.asset");

        AssetDatabase.Refresh();
        
        assetMap.Refresh();
    }
    
    [ContextMenu("Refresh all assets")]
    public void Refresh()
    {
        this.assets ??= new List<EntityObject>();
        this.assets?.Clear();
        var assets = EditorMap.GetEverything();

        foreach (var obj in assets)
        {
            var entityObject = new EntityObject(obj);
        
            this.assets.Add(entityObject);
        }

        AssetDatabase.Refresh();
        Debug.Log("Refreshed Assetmap with " + assets.Count  + " assets.");
    }

    IEnumerator Wait()
    {
 
//returning 0 will make it wait 1 frame
        yield return 0;
 
//code goes here
 
 
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

