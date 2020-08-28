using System;
using System.IO;
using System.Linq;
using DOTS.Serialization.ReferencedObjects;
using Unity.Assertions;
using Unity.Scenes;
using Hash128 = Unity.Entities.Hash128;
using UnityObject = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Object = UnityEngine.Object;

namespace ReferencedObjects
{
    public static class AssetUtility
    {
        /// <summary>
        /// Saves out 
        /// </summary>
        /// <param name="objects"></param>
#if UNITY_EDITOR
        [MenuItem("JSONSTUFF/CreateAssetMap")]
#endif
        public static void CreateJson(ReferencedUnityObjects objects)
        {
            var map = new DiskAssetMap();
            map.Create(objects);
            //Debug.Log(objects);
            
            var saveLocation = Application.persistentDataPath + "\\" + "Saves";

            if (!Directory.Exists(saveLocation))
            {
                Directory.CreateDirectory(saveLocation);
            }
            
            // json make asset map json file
            var path     = saveLocation + "\\" + "SerializedAssetMap.json";
            var jsondata = JsonUtility.ToJson(map, true);
            File.WriteAllText(path, jsondata);
            
            //Debug.Log("WriteAllText");
    #if UNITY_EDITOR
            AssetDatabase.Refresh();
    #endif
        }

        /// <summary>
        /// Saves out 
        /// </summary>
    #if UNITY_EDITOR
        [MenuItem("JSON1STUFF/LoadJsonAssetMap")]
    #endif
        public static ReferencedUnityObjects LoadJsonToUnity()
        {
            // Load the json file
            var json     = File.ReadAllText(Application.persistentDataPath + "/Saves/SerializedAssetMap.json");
            var assetMap = JsonUtility.FromJson<DiskAssetMap>(json);
            // Create instance of Entities ReferencedUnityObjects
            var instance = UnityEngine.ScriptableObject.CreateInstance<ReferencedUnityObjects>();
            instance.Array = new Object[assetMap.Array.Length];
            var array = new Object[assetMap.Array.Length];
            
                // Make a dummy file to test in editor
                var dummyGameObject = new GameObject();
                dummyGameObject.name = "Dummy";
                // DEBUGGING: Create dummy monobehaviour
                var debugJsonData = dummyGameObject.AddComponent<DebugJsonData>();
                debugJsonData.Array = new Object[assetMap.Array.Length];
            
            // Next get the actual object references and their ids from the persistent objects asset(note this asset should be created in editor or during a build).
            var assetArray = Resources.Load<AssetMap>("AssetMap").assets;// PrefabId.Instance().Assets;
            
            // Iterate every object in jsonified data 
            for (var i = 0; i < assetMap.Array.Length; i++)
            {
                // Iterate all objects in persistentObjects.asset
                // Linq expression that is apparently same as doing an if check for id
                foreach (var actualObject in assetArray.Where(actualObject => actualObject.hash == assetMap.Array[i].hash))
                {
                    var unityObject = actualObject.referencedObject;
                    // Match array index to object
                    array[i] = unityObject;
                        debugJsonData.Array[i] = unityObject;
                    instance.Array[i]      = unityObject;
                    
                }
            }
            
            return instance;
        }
    }
    
    /// <summary>
    /// The json class to save
    /// </summary>
    [Serializable]
    public class DiskAssetMap
    {
        public SerializedObjectHash[] Array;

        
        /// <summary>
        /// Create a matching serializable list of object ids from a Entities ReferencedUnityObjects instance when serializing.
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="referencedUnityObjects"></param>
        public void Create(ReferencedUnityObjects objects)
        {
            var referencedUnityObjects = objects;
            
            // First copy the length of the incoming ReferencedUnityObjects array
            Array = new SerializedObjectHash[referencedUnityObjects.Array.Length];
            
            
            // Loop through and find the matching object for each entry in the incoming referenced objects
            for (int i = 0; i < referencedUnityObjects.Array.Length; i++)
            {
                Array[i].hash = AssetMapUtility.GenerateHash(referencedUnityObjects.Array[i]);
                Array[i].name = referencedUnityObjects.Array[i].name;
            }
        }
    }
    
    /// <summary>
    /// A unique id, referencing a managed object.
    /// </summary>
    [Serializable]
    public struct SerializedObjectHash
    {
        // Unused for now, but the name of the UnityEngine.Object
        public string name;
        // A unique hash generated from the reference UnityEngine.Object
        public Hash128 hash;
    }
}