using System;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace DefaultNamespace.DummyScripts
{
    public class AddressableDataQuery : MonoBehaviour
    {
        public AddressableAssetGroup addressableAssetGroup;

        public void Start()
        {
            //AssetIdToAddress = new Dictionary<string, string>();
            //Debug.Log(UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings.groups.Count);
            Debug.Log(UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings.groups[1].entries.Count);

            
            foreach (var addressableAssetGroup in UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject
                .Settings.groups)
            {
                foreach (var addressableAssetEntry in addressableAssetGroup.entries)
                {
                    
                }
            }
return;
            Debug.Log(addressableAssetGroup.entries.Count);

            foreach (var entry in addressableAssetGroup.entries)
            {
                Debug.Log(entry.GetType());
                Debug.Log(entry.address);
                Debug.Log(entry.TargetAsset);
                //Debug.Log(entry.guid);
            }
        }
    }
}