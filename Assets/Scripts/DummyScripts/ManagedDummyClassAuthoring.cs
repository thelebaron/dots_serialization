using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class ManagedDummyClassAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public int DummyValue;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentObject(entity, new DummyManagedClassComponent{DummyValue = DummyValue});
    }
}

[Serializable]
public class DummyManagedClassComponent : IComponentData
{
    public int DummyValue;
}