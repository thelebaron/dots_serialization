using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;
using Hash128 = Unity.Entities.Hash128;
using UnityObject = UnityEngine.Object;
#if UNITY_EDITOR
using System.Collections;
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
        var assets = EditorAssetDatabaseUtility.GetEverything();

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

