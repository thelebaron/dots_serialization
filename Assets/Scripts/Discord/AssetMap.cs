using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
#endif


namespace Game.Mods
{
    public class AssetMap : ScriptableObject
    {
        [SerializeField] public Dictionary<string, string> AssetIdToAddress { get; private set; }

        public static string GetKey(Object obj)
        {
            //var guid = StringToGUID(name);
            var name = obj.name;
            var type = obj.GetType();

            return name + type.FullName;
        }
#if UNITY_EDITOR

        
        public void UpdateMap()
        {
            AssetIdToAddress = new Dictionary<string, string>();

            foreach (var addressableAssetGroup in UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject
                .Settings.groups)
            {
                foreach (var addressableAssetEntry in addressableAssetGroup.entries)
                {
                    if (addressableAssetEntry.TargetAsset != null)
                    {
                        var addressables = new List<AddressableAssetEntry>();
                        addressableAssetEntry.GatherAllAssets(addressables, false, true, true);

                        if (addressableAssetEntry.address.Contains("Character"))
                        {
                            Debug.Log(addressables.Count);
                        }

                        var objs = AssetDatabase.LoadAllAssetRepresentationsAtPath(addressableAssetEntry.AssetPath);

                        foreach (var subObject in objs)
                        {
                            var stringKey = GetKey(subObject);

                            if (!AssetIdToAddress.ContainsKey(stringKey))
                                AssetIdToAddress.Add(stringKey,
                                    addressableAssetEntry.address + "[" + subObject.name + "]");
                        }

                        var key = GetKey(addressableAssetEntry.TargetAsset);
                        if (!AssetIdToAddress.ContainsKey(key))

                            AssetIdToAddress.Add(GetKey(addressableAssetEntry.TargetAsset),
                                addressableAssetEntry.address);
                        else
                        {
                            Debug.Log($"Contains {key} --- {addressableAssetEntry.address}");
                        }
                    }
                }
            }
        }

#endif
    }
}