/*
 * Looks for any entities that contain the "Missing" component tags and replaces them with the actual components from
 * the associated prefab for that entity.
 */

using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Rendering;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public class RestoreAfterLoadSystem : SystemBase
{
    private EntityCommandBufferSystem _commandBufferSystem;
    private PrefabSystem _prefabSystem;

    protected override void OnCreate()
    {
        _commandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        _prefabSystem = World.GetOrCreateSystem<PrefabSystem>();
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer commandBuffer = _commandBufferSystem.CreateCommandBuffer();

        NativeHashMap<FixedString32, Entity> prefabs = _prefabSystem.PrefabsByID;

        Entities
            .WithAll<RenderMeshRemoved>()
            .WithoutBurst()
            .ForEach(
                (Entity entity, in SaveEntity prefabID) =>
                {
                    if (!prefabs.TryGetValue(prefabID.Identifier, out Entity prefab))
                    {
                        Debug.LogError($"Could not find prefab with id {prefabID.Identifier}");
                        return;
                    }
                    var renderMesh = EntityManager.GetSharedComponentData<RenderMesh>(prefab);
                    var description = new RenderMeshDescription(renderMesh.mesh, renderMesh.material);
                    RenderMeshUtility.AddComponents(entity, commandBuffer, description);
                    commandBuffer.RemoveComponent<RenderMeshRemoved>(entity);
                }
            ).Run();
        
        Entities
            .WithAll<PhysicsColliderRemoved>()
            .ForEach(
                (Entity entity, in SaveEntity prefabID) =>
                {
                    if (!prefabs.TryGetValue(prefabID.Identifier, out Entity prefab))
                    {
                        Debug.LogError($"Could not find prefab with id {prefabID.Identifier}");
                        return;
                    }
                    commandBuffer.AddComponent(entity, GetComponent<PhysicsCollider>(prefab));
                    commandBuffer.RemoveComponent<PhysicsColliderRemoved>(entity);
                }
            ).Schedule();
        
        Entities
            .WithAll<MissingMyBlobComponentTag>()
            .ForEach(
                (Entity entity, in SaveEntity prefabID) =>
                {
                    if (!prefabs.TryGetValue(prefabID.Identifier, out Entity prefab))
                    {
                        Debug.LogError($"Could not find prefab with id {prefabID.Identifier}");
                        return;
                    }
                    commandBuffer.AddComponent(entity, GetComponent<MyBlobComponent>(prefab));
                    commandBuffer.RemoveComponent<MissingMyBlobComponentTag>(entity);
                }
            ).Schedule();

        _commandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}