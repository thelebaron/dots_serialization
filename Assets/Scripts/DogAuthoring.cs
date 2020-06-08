using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
    public class DogAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new Dog {GoodGirl = true});
        }
    }

    public struct Dog : IComponentData
    {
        public bool GoodGirl;
    }
}