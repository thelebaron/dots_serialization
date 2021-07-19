using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Utility;
using Hash128 = Unity.Entities.Hash128;

public class DummySaveAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        //var dummy = new DummyComponent {ints = ints, Vector3s = new Vector3[10], MyDataFormats = dataformat};
        //var Hash128       = GuidUtility.GenerateGuid(dummy);
        

        dstManager.AddComponent<SerializeableTag>(entity);
    }
}

[Serializable]
public class DummyComponent : IComponentData
{
    public int[]          ints;
    public Vector3[]      Vector3s;
    public MyDataFormat[] MyDataFormats;
    public Hash128        Hash128;
}

[Serializable]
public struct MyDataFormat
{
    public float PointA;
    public float PointB;
}