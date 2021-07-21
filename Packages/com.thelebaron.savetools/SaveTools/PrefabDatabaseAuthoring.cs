using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class PrefabDatabaseAuthoring : MonoBehaviour,IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    [SerializeField] public List<GameObject> prefabs;
    public List<GameObject> Prefabs
    {
        get => prefabs;
        set => prefabs = value;
    }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddBuffer<SerializeablePrefabs>(entity);
        var buffer = dstManager.GetBuffer<SerializeablePrefabs>(entity);
        for (int i = 0; i < prefabs.Count; i++)
        {
            buffer.Add(new SerializeablePrefabs
            {
                Prefab =conversionSystem.GetPrimaryEntity(prefabs[i])
                
            });
        }
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        for (var index = 0; index < prefabs.Count; index++)
        {
            var prefab = prefabs[index];
            referencedPrefabs.Add(prefab);
        }
    }
}

public struct SerializeablePrefabs : IBufferElementData
{
    public Entity Prefab;
}
