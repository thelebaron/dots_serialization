using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DOTS.Serialization.ReferencedObjects;
using Unity.Assertions;
using Unity.Mathematics;
using Unity.Scenes;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Object = UnityEngine.Object;

namespace ReferencedObjects
{
    public static class NewAssetUtility
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
            var map = new AssetMap();
            map.Create(objects);
            
            var saveLocation = Application.persistentDataPath + "\\" + "Saves";
        
            if (!Directory.Exists(saveLocation))
                Directory.CreateDirectory(saveLocation);
            
            // json make asset map json file
            var path     = saveLocation + "\\" + "AssetMap.json";
            var jsondata = JsonUtility.ToJson(map, true);
            File.WriteAllText(path, jsondata);
            
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }
        
        /// <summary>
        /// Saves out 
        /// </summary>
#if UNITY_EDITOR
        [MenuItem("JSONSTUFF/LoadJsonAssetMap")]
#endif
        public static ReferencedUnityObjects LoadJsonToUnity()
        {
            // Load the json file
            //Debug.Log("try path = " + Application.persistentDataPath + "/Saves/AssetMap.json" );
            var json     = File.ReadAllText(Application.persistentDataPath + "/Saves/AssetMap.json");
            var assetMap = JsonUtility.FromJson<AssetMap>(json);
            
            // Make a dummy file to test in editor
            var dummyGameObject = new GameObject();
            dummyGameObject.name = "Dummy";
            
            // Create instance of Entities ReferencedUnityObjects
            var instance = UnityEngine.ScriptableObject.CreateInstance<ReferencedUnityObjects>();
            instance.Array = new Object[assetMap.Array.Length];
            var array = new Object[assetMap.Array.Length];
            
            // DEBUGGING: Create dummy monobehaviour
            var debugJsonData = dummyGameObject.AddComponent<DebugJsonData>();
            debugJsonData.Array = new Object[assetMap.Array.Length];
            
            // Next get the actual object references and their ids from the persistent objects asset(note this asset should be created in editor or during a build).
            /*#if UNITY_EDITOR
            var persistentObjects = AssetDatabase.LoadAssetAtPath<PersistentObjects>("Assets/Resources/PersistentObjects.asset");
            #endif*/
            var persistentObjects = PrefabId.Instance().PersistentObjects;
            //Assert.IsNotNull(persistentObjects);
            var assetArray = persistentObjects.Assets;// PrefabId.Instance().Assets;
            
            // Iterate every object in jsonified data 
            for (var i = 0; i < assetMap.Array.Length; i++)
            {
                // Iterate all objects in persistentObjects.asset
                // Linq expression that is apparently same as doing an if check for id
                foreach (var actualObject in assetArray.Where(actualObject => actualObject.id == assetMap.Array[i].id))
                {
                    var unityObject = actualObject.referencedObject;
                    // Match array index to object
                    array[i] = unityObject;
                    debugJsonData.Array[i] = unityObject;
                    instance.Array[i]      = unityObject;
                    
                }
            }

            foreach (var VARIABLE in debugJsonData.Array)
            {
                //Assert.IsNotNull(VARIABLE);
            }

            //debugJsonData.Array = array;
            //instance.Array = array;
            
            return instance;
        }
    }
    
    /// <summary>
    /// The json class to save
    /// </summary>
    [Serializable]
    public class AssetMap
    {
        public JsonObject[] Array;

        
        /// <summary>
        /// Create a matching serializable list of object ids from a Entities ReferencedUnityObjects instance when serializing.
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="referencedUnityObjects"></param>
        public void Create(ReferencedUnityObjects objects)
        {
            Assert.IsNotNull(objects);
            Assert.IsNotNull(objects.Array);
            
            
            var referencedUnityObjects = objects;
            
            // First copy the length of the incoming ReferencedUnityObjects array
            Array = new JsonObject[referencedUnityObjects.Array.Length];
            
            /*// Next get the actual object references and their ids from the persistent objects asset(note this asset should be created in editor or during a build).
            var persistentObjects = AssetDatabase.LoadAssetAtPath<PersistentObjects>("Assets/PersistentObjects.asset");
            Assert.IsNotNull(persistentObjects);*/
            
            // Loop through and find the matching object for each entry in the incoming referenced objects
            for (int i = 0; i < referencedUnityObjects.Array.Length; i++)
            {
                Array[i].id = EditorAssetDatabaseUtility.GenerateHash(referencedUnityObjects.Array[i]).ToString();
                //Array[i].Id = referencedUnityObjects.Array[i].GetHashCode();
                Array[i].Name = referencedUnityObjects.Array[i].name;
            }
        }
    }
    
    /// <summary>
    /// A unique id, referencing a managed object.
    /// </summary>
    [Serializable]
    public struct JsonObject
    {
        public string Name;
        public string id;
    }
}