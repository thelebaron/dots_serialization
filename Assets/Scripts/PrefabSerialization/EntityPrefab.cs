using System;
using Unity.Entities;
using UnityEngine;

    [DisallowMultipleComponent]
    public class EntityPrefab : MonoBehaviour, IConvertGameObjectToEntity
    {
        public string guid;
        
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new SerializePrefab {Value = guid});
        }
    }
