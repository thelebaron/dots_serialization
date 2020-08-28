using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Assertions;
using UnityEngine;
using UnityEngine.Serialization;


public class PrefabId : MonoBehaviour
{
    public AssetMap assetMap;
    
    public List<EntityObject> Assets;

    public static PrefabId Instance()
    {
        return instance;
    }
    
    private static PrefabId instance;
    
    private void Awake()
    {
        instance = this;

        Assets = assetMap.assets;
    }
}
