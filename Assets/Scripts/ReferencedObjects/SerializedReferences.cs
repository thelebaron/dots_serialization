using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Assertions;
using Unity.Scenes;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ReferencedObjects
{
    public static class NewAssetMapUtility
    {
        /// <summary>
        /// Saves out 
        /// </summary>
        [MenuItem("JSONSTUFF/CreateAssetMap")]
        public static void CreateJson()
        {
            var map = new AssetMap();
            map.Create();
            
            if (!Directory.Exists("Assets/Saves"))
                Directory.CreateDirectory("Assets/Saves");
            // json make asset map json file
            var path     = "Assets" + "\\"+ "Saves"+ "\\" + "AssetMap.json";
            var jsondata = JsonUtility.ToJson(map, true);
            File.WriteAllText(path, jsondata);
            
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }
        
        /// <summary>
        /// Saves out 
        /// </summary>
        [MenuItem("JSONSTUFF/LoadJsonAssetMap")]
        public static void LoadJsonToUnity()
        {
            // Load the json file
            var json     = File.ReadAllText(Application.dataPath + "/Saves/AssetMap.json");
            var assetMap = JsonUtility.FromJson<AssetMap>(json);
            
            // Make a dummy file to test in editor
            var dummyGameObject = new GameObject();
            dummyGameObject.name = "Dummy";
            // Add dummy monobehaviour
            var jdata = dummyGameObject.AddComponent<DummyJsonifiedData>();
            jdata.Array = new Object[assetMap.Array.Length];
            
            // Next get the actual object references and their ids from the persistent objects asset(note this asset should be created in editor or during a build).
            var persistentObjects = AssetDatabase.LoadAssetAtPath<PersistentObjects>("Assets/PersistentObjects.asset");
            Assert.IsNotNull(persistentObjects);
            var actualAssets = persistentObjects.Assets;
            
            // Iterate every object in jsonified data 
            for (var i = 0; i < assetMap.Array.Length; i++)
            {
                var jsonObject = assetMap.Array[i];
                var id = jsonObject.Id;

                // Iterate all objects in persistentObjects.asset
                // Linq expression that is apparently same as doing an if check for id
                foreach (var actualObject in actualAssets.Where(actualObject => actualObject.id == id))
                {
                    // Match array index to object
                    jdata.Array[i] = actualObject.referencedObject;
                    break;
                }
            }
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
        /// <param name="referencedUnityObjects"></param>
        public void Create()
        {
            var referencedUnityObjects = AssetDatabase.LoadAssetAtPath<ReferencedUnityObjects>("Assets/UnityObjects.asset");
            
            // First copy the length of the incoming ReferencedUnityObjects array
            Array = new JsonObject[referencedUnityObjects.Array.Length];
            
            /*// Next get the actual object references and their ids from the persistent objects asset(note this asset should be created in editor or during a build).
            var persistentObjects = AssetDatabase.LoadAssetAtPath<PersistentObjects>("Assets/PersistentObjects.asset");
            Assert.IsNotNull(persistentObjects);*/
            
            // Loop through and find the matching object for each entry in the incoming referenced objects
            for (int i = 0; i < referencedUnityObjects.Array.Length; i++)
            {
                Array[i].Id = referencedUnityObjects.Array[i].GetHashCode();
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
        public int Id;
    }
}