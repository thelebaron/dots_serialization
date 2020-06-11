using System;
using System.Collections.Generic;
using UnityEngine;

namespace DOTS.Serialization
{
    /// <summary>
    /// A map listing of a key: the name+type and address: addressable address
    /// </summary>
    [System.Serializable]
    public class AssetMap
    {
        [SerializeField] public List<AssetKey> AssetMapping = new List<AssetKey>();// { get; private set; }
        [System.Serializable]
        public class AssetKey
        {
            public string Key;
            public string Address;
        }
    }

    /// <summary>
    /// Stores a list of UObjects
    /// </summary>
    [System.Serializable]
    public class WObjects
    {
        public UObject[] Objects;
    }
    /// <summary>
    /// Managed UnityObject with a type and an addressable address.
    /// </summary>
    [System.Serializable]
    public class UObject
    {
        public string Address { get; set; }
        public Type Type { get; set; }
    }
}