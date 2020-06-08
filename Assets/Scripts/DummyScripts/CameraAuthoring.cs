using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public bool UseCompanionConversion;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        if (UseCompanionConversion)
        {
            conversionSystem.AddHybridComponent(GetComponent<Camera>());
        }
        else
        {
            dstManager.AddComponentObject(entity, GetComponent<Camera>());
        }
        dstManager.AddComponentData(entity, new CameraDummy{DummyValue = true, SecondDummyValue = true});
    }
}

public struct CameraDummy : IComponentData
{
    public bool DummyValue;
    public bool SecondDummyValue;
}