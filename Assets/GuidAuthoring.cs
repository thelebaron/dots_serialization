using System;
using Unity.Entities;
using UnityEngine;

public class GuidAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new GuidComponent{ Value = Guid.NewGuid() });
    }
}

public struct GuidComponent : IComponentData
{
    public Guid Value;
}
