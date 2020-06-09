using System;
using System.Collections.Generic;
using UnityEngine;

namespace DOTS.Serialization
{
    [System.Serializable]
    public class AssetMap
    {
        [SerializeField] public List<AssetKey> AssetMapping;// { get; private set; }
        [System.Serializable]
        public class AssetKey
        {
            public string Key;
            public string Address;
        }
    }


}