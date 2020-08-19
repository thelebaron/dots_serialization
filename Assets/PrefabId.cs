using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Assertions;
using UnityEngine;


public class PrefabId : MonoBehaviour
{
    public PersistentObjects PersistentObjects;
    
    public List<EntityObject> Assets;

    public static PrefabId Instance()
    {
        return instance;
    }
    
    private static PrefabId instance;
    
    private void Awake()
    {
        instance = this;

        Assets = PersistentObjects.Assets;
    }
}
