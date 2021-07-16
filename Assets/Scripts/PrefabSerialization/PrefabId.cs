using System;
using Unity.Entities;
using UnityEngine;

namespace PrefabSerialization
{
    [DisallowMultipleComponent]
    public class PrefabId : MonoBehaviour, IConvertGameObjectToEntity
    {
        public string guid;
        
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new SerializePrefab {Value = guid});
        }
    }
}