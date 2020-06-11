using System.Collections.Generic;
using System.IO;
using DefaultNamespace;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace DOTS.Serialization
{
    public static class AssetMapUtilities
    { 
        /// <summary>
        /// Creates a string key based on the objects name and type
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetKey(UnityEngine.Object obj)
        {
            //var guid = StringToGUID(name);
            var name = obj.name;
            var type = obj.GetType();

            return name + type.FullName;
        }
        
        /// <summary>
        /// Updates assetmap with all current addressables in the default addressableassetentries group.
        /// </summary>
        /// <returns></returns>
        [MenuItem("Serialize/UpdateAssetMap")]
        public static AssetMap UpdateAssetMap()
        {
            //use default group for now
            //Debug.Log(UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings.groups[1].entries.Count);
            
            var map = new AssetMap();

            var addressableAssetEntries = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings
                .groups[1].entries;//DefaultAddressableAssetEntries();
            
            foreach (var assetEntry in addressableAssetEntries)
            {
                Debug.Log(assetEntry.MainAsset);
                var id = GetAssetKey(assetEntry);
                var key = GetKey(assetEntry.MainAsset);
                var address = assetEntry.address;

                if (!map.AssetMapping.Contains(GetAssetKey(assetEntry)))
                {
                    map.AssetMapping.Add(GetAssetKey(assetEntry));
                }
                
            }

            return map;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assetEntry"></param>
        /// <returns></returns>
        public static AssetMap.AssetKey GetAssetKey(AddressableAssetEntry assetEntry)
        {
            var key = GetKey(assetEntry.MainAsset);
            var address = assetEntry.address;
            return new AssetMap.AssetKey {Key = key, Address = address};
        }
        
        /// <summary>
        /// Returns the default group for addressable entries
        /// </summary>
        /// <returns></returns>
        public static ICollection<AddressableAssetEntry> DefaultAddressableAssetEntries()
        {
            return UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings.groups[1].entries;
        }
        
        /// <summary>
        /// Saves out 
        /// </summary>
        [MenuItem("Serialize/CreateAssetMap")]
        public static void CreateJsonAssetMap()
        {
            if (!Directory.Exists("Assets/Saves"))
                Directory.CreateDirectory("Assets/Saves");
            // json make asset map json file
            var path = "Assets" + "\\"+ "Saves"+ "\\" + "AssetMap.json";
            var jsondata = JsonUtility.ToJson(UpdateAssetMap(), true);
            File.WriteAllText(path, jsondata);
            
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }

        /// <summary>
        /// Currently just an editor menu method to test whether loading json file is parsed correctly
        /// </summary>
        [MenuItem("Serialize/ParseAssetMap")]
        public static void ParseJsonAssetMap()
        {
            var json = File.ReadAllText(Application.dataPath + "/Saves/AssetMap.json");
            AssetMap assetMap = JsonUtility.FromJson<AssetMap>(json);
            Debug.Log(assetMap);
            foreach (var pair in assetMap.AssetMapping)
            {
                var key = pair.Key;
                var address = pair.Address;
                Debug.Log(pair.Key + " <key - address> " + address);
                
                var load = Addressables.LoadAssetAsync<Object>(address);
                //yield return load; // i think this is necessary
                var result = load.Result;
                if(result!=null)
                    Debug.Log(result);
            }
        }
        
        /// <summary>
        /// Returns the last saved asset map.
        /// </summary>
        /// <returns></returns>
        public static AssetMap GetSavedAssetMap()
        {
            // unsure but just recreates the assetmap just in case not up to date. perf might degrade as this gets bigger
            CreateJsonAssetMap();
            
            var json = File.ReadAllText(Application.dataPath + "/Saves/AssetMap.json");
            AssetMap assetMap = JsonUtility.FromJson<AssetMap>(json);
            
            /*foreach (var assetkey in assetMap.AssetMapping)
            {
                Debug.Log(assetkey.Key + " <key - address> " + assetkey.Address);
                
                var load = Addressables.LoadAssetAsync<Object>(assetkey.Address);
                //yield return load; // i think this is necessary
                var result = load.Result;
                if(result!=null)
                    Debug.Log(result);
            }*/

            return assetMap;
        }

    }
}