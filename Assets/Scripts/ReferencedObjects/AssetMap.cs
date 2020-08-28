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
    //public UnityObject Target;
    //public ReferencedUnityObjects ReferencedUnityObjects;
    [FormerlySerializedAs("Assets")] public List<EntityObject> assets;

   /* public static ReferencedUnityObjects RemappedObjects()
    {
        var objRefs = UnityEngine. ScriptableObject. CreateInstance<ReferencedUnityObjects>();
        // find and compare with json
        return null;
    }*/
    
#if UNITY_EDITOR
    
    [MenuItem("SerializeNew/Create or Refresh AssetMap")]
    public static void CreateAsset()
    {
        AssetDatabase.CreateAsset(CreateInstance<AssetMap>(), "Assets/Resources/AssetMap.asset");
        var asset = AssetDatabase.LoadAssetAtPath<AssetMap>("Assets/Resources/AssetMap.asset");
        asset.Refresh();
    }
    
    /*/// <summary>
    /// Convert a referenced object to unique id
    /// </summary>
    [ContextMenu("ConvertTarget")]
    public void ConvertObject()
    {
        var entityObject = new EntityObject
        {
            referencedObject = Target, 
            id = Guid.Parse(Target.name + Target.GetType()).ToString()
        };

        //var id = Target.GetHashCode() + Target.GetInstanceID();

        //entityObject.id = id;
        Assets.Add(entityObject);
    }*/

    /*/// <summary>
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
                var formatter = new BinaryFormatter();
                formatter.Serialize(s, o);
                size = s.Length;
            }

            Debug.Log("asset name: " + asset.name + " | size: " + size);//+ " | hash: " +  asset.GetHashCode());
            
        }
    }
    */
    [ContextMenu("GetEveryAsset")]
    public void Refresh()
    {
        this.assets ??= new List<EntityObject>();
        this.assets?.Clear();
        var assets = EditorAssetDatabaseUtility.GetEverything();

        foreach (var obj in assets)
        {
            var entityObject = new EntityObject(obj);
        
            //entityObject.referencedObject = obj;
            
            //entityObject.id = EditorAssetDatabaseUtility.GenerateHash(obj).ToString();
            //entityObject.hash = EntityObject.
            
            this.assets.Add(entityObject);
        }
    }



#endif
}

[Serializable]
public class EntityObject
{
    public UnityObject referencedObject;
    //public string id;
    //public int guid;
    
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

