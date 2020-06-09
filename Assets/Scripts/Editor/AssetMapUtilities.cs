using System.Collections.Generic;
using System.IO;
using DefaultNamespace;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace DOTS.Serialization
{
    public static class AssetMapUtilities
    { 
        public static string GetKey(UnityEngine.Object obj)
        {
            //var guid = StringToGUID(name);
            var name = obj.name;
            var type = obj.GetType();

            return name + type.FullName;
        }
        
        [MenuItem("Serialize/UpdateAddressables")]
        public static AssetMap UpdateAddressables()
        {
            //use default group for now
            //Debug.Log(UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings.groups[1].entries.Count);
            
            var map = new AssetMap
            {
                AssetMapping = new List<AssetMap.AssetKey>()
            };

            var addressableAssetEntries = 
                UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings.groups[1].entries;
            
            foreach (var assetEntry in addressableAssetEntries)
            {
                var key = GetKey(assetEntry.MainAsset);
                Debug.Log(assetEntry.MainAsset);
                var address = assetEntry.address;

                var id = new AssetMap.AssetKey {Key = key, Address = address};
                if(!map.AssetMapping.Contains(id))
                    map.AssetMapping.Add(id);
                
            }

            return map;
        }

        [MenuItem("Serialize/CreateAssetMap")]
        public static void CreateJsonAssetMap()
        {
            if (!Directory.Exists("Assets/Saves"))
                Directory.CreateDirectory("Assets/Saves");
            // json make asset map json file
            var path = "Assets" + "\\"+ "Saves"+ "\\" + "AssetMap.json";
            var jsondata = JsonUtility.ToJson(AssetMapUtilities.UpdateAddressables(), true);
            File.WriteAllText(path, jsondata);
            
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }

        [MenuItem("Serialize/ParseAssetMap")]
        public static void ParseJsonAssetMap()
        {
            var json = File.ReadAllText(Application.dataPath + "/Saves/AssetMap.json");
            AssetMap contentData = JsonUtility.FromJson<AssetMap>(json);
            
            foreach (var assetkey in contentData.AssetMapping)
            {
                Debug.Log(assetkey.Key + " <key - address> " + assetkey.Address);
                
                var load = Addressables.LoadAssetAsync<Object>(assetkey.Address);
                //yield return load; // i think this is necessary
                var result = load.Result;
                if(result!=null)
                    Debug.Log(result);
            }
        }

    }
}