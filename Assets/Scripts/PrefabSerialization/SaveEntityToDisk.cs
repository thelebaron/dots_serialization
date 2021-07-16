using System;
using Unity.Entities;
using UnityEngine;

    [DisallowMultipleComponent]
    [AddComponentMenu("DOTS/Save Entity (Serialize)")]
    public class SaveEntityToDisk : MonoBehaviour, IConvertGameObjectToEntity
    {
        public string guid;
        
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            if(guid.Length.Equals(0))
                Debug.LogError("Error: Save entity guid is zero");
            dstManager.AddComponentData(entity, new SaveEntity {Value = guid});
        }
    }
