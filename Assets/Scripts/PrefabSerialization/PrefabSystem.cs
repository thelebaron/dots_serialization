/*
 * Stores all prefabs that contain a PrefabID in a hash map keyed by ID that is accessible by other systems.
 */

using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[UpdateAfter(typeof(ConvertToEntitySystem))]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public class PrefabSystem : SystemBase
{
    public NativeHashMap<FixedString32, Entity> PrefabsByID => prefabHashMap;
    private NativeHashMap<FixedString32, Entity> prefabHashMap;
    private EntityCommandBufferSystem entityCommandBufferSystem;

    private struct SaveEntitySystemState : ISystemStateComponentData { }

    protected override void OnCreate()
    {
        prefabHashMap          = new NativeHashMap<FixedString32, Entity>(2, Allocator.Persistent);
        entityCommandBufferSystem = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
    }

    protected override void OnDestroy()
    {
        prefabHashMap.Dispose();
    }

    protected override void OnUpdate()
    {
        var prefabs       = prefabHashMap;
        var                  commandBuffer = entityCommandBufferSystem.CreateCommandBuffer();
        
        // Add all new prefabs to the prefab hash map.
        Entities
            .WithAll<Prefab>()
            .WithNone<SaveEntitySystemState>()
            .ForEach((Entity entity, in SaveEntity saveEntity) =>
                {
                    //Debug.Log(saveEntity.Identifier);
                    prefabs.Add(saveEntity.Identifier, entity);
                    commandBuffer.AddComponent<SaveEntitySystemState>(entity);
                }).Schedule();

        entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}