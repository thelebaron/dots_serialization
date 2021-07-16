/*
 * Stores all prefabs that contain a PrefabID in a hash map keyed by ID that is accessible by other systems.
 */

using Unity.Collections;
using Unity.Entities;

[UpdateAfter(typeof(ConvertToEntitySystem))]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public class SerializePrefabSystem : SystemBase
{
    private NativeHashMap<FixedString32, Entity> prefabHashMap;
    private EntityCommandBufferSystem entityCommandBufferSystem;

    private struct PrefabSystemState : ISystemStateComponentData { }

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
            .WithNone<PrefabSystemState>()
            .ForEach((Entity entity, in SaveEntity serializeablePrefab) =>
                {
                    prefabs.Add(serializeablePrefab.Value, entity);
                    commandBuffer.AddComponent<PrefabSystemState>(entity);
                }).Schedule();
        
        entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}