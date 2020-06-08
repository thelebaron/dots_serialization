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