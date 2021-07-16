using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Entities.Editor.Tests
{
    class ComponentDataDifferTests
    {
        World m_World;
        NativeList<Entity> m_NewEntities;
        NativeList<Entity> m_MissingEntities;
        NativeList<byte> m_Storage;
        EntityDiffer m_EntityDiffer;
        ComponentDataDiffer m_ChunkDiffer;

        [SetUp]
        public void Setup()
        {
            m_World = new World("TestWorld");
            m_NewEntities = new NativeList<Entity>(Allocator.TempJob);
            m_MissingEntities = new NativeList<Entity>(Allocator.TempJob);
            m_Storage = new NativeList<byte>(Allocator.TempJob);
            m_EntityDiffer = new EntityDiffer(m_World);
            m_ChunkDiffer = new ComponentDataDiffer(typeof(EcsTestData));
        }

        [TearDown]
        public void TearDown()
        {
            m_World.Dispose();
            m_NewEntities.Dispose();
            m_Storage.Dispose();
            m_MissingEntities.Dispose();
            m_EntityDiffer.Dispose();
            m_ChunkDiffer.Dispose();
        }

        [Test]
        public void ComponentDataDiffer_Simple()
        {
            var result = m_ChunkDiffer.GatherComponentChangesAsync(m_World.EntityManager.UniversalQuery, Allocator.TempJob, out var jobHandle);
            jobHandle.Complete();

            Assert.That(result.AddedComponentsCount, Is.EqualTo(0));
            Assert.That(result.RemovedComponentsCount, Is.EqualTo(0));

            result.Dispose();
        }

        [Test]
        public unsafe void ComponentDataDiffer_DetectNewEmptyEntityInArchetype()
        {
            var entityA = CreateEntity(new EcsTestData { value = 12 });

            using (var result = m_ChunkDiffer.GatherComponentChangesAsync(m_World.EntityManager.UniversalQuery, Allocator.TempJob, out var jobHandle))
            {
                jobHandle.Complete();

                Assert.That(result.AddedComponentsCount, Is.EqualTo(1));
                Assert.That(result.RemovedComponentsCount, Is.EqualTo(0));

                Assert.That(result.GetAddedComponent<EcsTestData>(0), Is.EqualTo((entityA, new EcsTestData { value = 12 })));
            }

            m_World.EntityManager.GetCheckedEntityDataAccess()->EntityComponentStore->IncrementGlobalSystemVersion();
            var entityB = m_World.EntityManager.CreateEntity(typeof(EcsTestData));
            using (var result = m_ChunkDiffer.GatherComponentChangesAsync(m_World.EntityManager.UniversalQuery, Allocator.TempJob, out var jobHandle))
            {
                jobHandle.Complete();

                Assert.That(result.AddedComponentsCount, Is.EqualTo(1));
                Assert.That(result.RemovedComponentsCount, Is.EqualTo(0));

                Assert.That(result.GetAddedComponent<EcsTestData>(0), Is.EqualTo((entityB, default(EcsTestData))));
            }
        }

        [Test]
        public unsafe void ComponentDataDiffer_DetectMissingComponentWhenEntityDestroyed()
        {
            var entityA = CreateEntity(new EcsTestData { value = 12 });
            var entityB = CreateEntity(new EcsTestData { value = 12 });

            using (m_ChunkDiffer.GatherComponentChangesAsync(m_World.EntityManager.UniversalQuery, Allocator.TempJob, out var jobHandle))
            {
                jobHandle.Complete();
            }

            m_World.EntityManager.GetCheckedEntityDataAccess()->EntityComponentStore->IncrementGlobalSystemVersion();
            m_World.EntityManager.DestroyEntity(entityB);

            using (var result = m_ChunkDiffer.GatherComponentChangesAsync(m_World.EntityManager.UniversalQuery, Allocator.TempJob, out var jobHandle))
            {
                jobHandle.Complete();

                Assert.That(result.AddedComponentsCount, Is.EqualTo(0));
                Assert.That(result.RemovedComponentsCount, Is.EqualTo(1));
            }
        }

        [Test]
        public unsafe void ComponentDataDiffer_DetectNewAndMissing()
        {
            var entityA = CreateEntity(new EcsTestData { value = 12 });

            using (var result = m_ChunkDiffer.GatherComponentChangesAsync(m_World.EntityManager.UniversalQuery, Allocator.TempJob, out var jobHandle))
            {
                jobHandle.Complete();

                Assert.That(result.AddedComponentsCount, Is.EqualTo(1));
                Assert.That(result.RemovedComponentsCount, Is.EqualTo(0));

                Assert.That(result.GetAddedComponent<EcsTestData>(0), Is.EqualTo((entityA, new EcsTestData { value = 12 })));
            }

            m_World.EntityManager.GetCheckedEntityDataAccess()->EntityComponentStore->IncrementGlobalSystemVersion();
            var entityB = CreateEntity(new EcsTestData { value = 22 });
            using (var result = m_ChunkDiffer.GatherComponentChangesAsync(m_World.EntityManager.UniversalQuery, Allocator.TempJob, out var jobHandle))
            {
                jobHandle.Complete();

                Assert.That(result.AddedComponentsCount, Is.EqualTo(1));
                Assert.That(result.RemovedComponentsCount, Is.EqualTo(0));

                Assert.That(result.GetAddedComponent<EcsTestData>(0), Is.EqualTo((entityB, new EcsTestData { value = 22 })));
            }

            m_World.EntityManager.GetCheckedEntityDataAccess()->EntityComponentStore->IncrementGlobalSystemVersion();
            m_World.EntityManager.DestroyEntity(entityA);
            var entityC = CreateEntity(new EcsTestData { value = 32 });

            using (var result = m_ChunkDiffer.GatherComponentChangesAsync(m_World.EntityManager.UniversalQuery, Allocator.TempJob, out var jobHandle))
            {
                jobHandle.Complete();

                Assert.That(result.AddedComponentsCount, Is.EqualTo(2));
                Assert.That(result.RemovedComponentsCount, Is.EqualTo(2));

                Assert.That(result.GetAddedComponent<EcsTestData>(0), Is.EqualTo((entityB, new EcsTestData { value = 22 })));
                Assert.That(result.GetAddedComponent<EcsTestData>(1), Is.EqualTo((entityC, new EcsTestData { value = 32 })));
                Assert.That(result.GetRemovedComponent<EcsTestData>(0), Is.EqualTo((entityA, new EcsTestData { value = 12 })));
                Assert.That(result.GetRemovedComponent<EcsTestData>(1), Is.EqualTo((entityB, new EcsTestData { value = 22 })));
            }
        }

        [Test]
        public unsafe void ComponentDataDiffer_DetectChangedAsNewAndRemoved()
        {
            var entityA = CreateEntity(new EcsTestData { value = 12 });
            var entityB = CreateEntity(new EcsTestData { value = 22 });

            using (m_ChunkDiffer.GatherComponentChangesAsync(m_World.EntityManager.UniversalQuery, Allocator.TempJob, out var jobHandle))
            {
                jobHandle.Complete();
            }

            m_World.EntityManager.GetCheckedEntityDataAccess()->EntityComponentStore->IncrementGlobalSystemVersion();
            m_World.EntityManager.SetComponentData(entityA, new EcsTestData { value = 32 });

            using (var result = m_ChunkDiffer.GatherComponentChangesAsync(m_World.EntityManager.UniversalQuery, Allocator.TempJob, out var jobHandle))
            {
                jobHandle.Complete();
                Assert.That(result.AddedComponentsCount, Is.EqualTo(1));
                Assert.That(result.RemovedComponentsCount, Is.EqualTo(1));

                Assert.That(result.GetAddedComponent<EcsTestData>(0), Is.EqualTo((entityA, new EcsTestData { value = 32 })));
                Assert.That(result.GetRemovedComponent<EcsTestData>(0), Is.EqualTo((entityA, new EcsTestData { value = 12 })));
            }
        }

        [Test]
        public unsafe void ComponentDataDiffer_ResultShouldntBeInterlaced()
        {
            var entityA = CreateEntity(new EcsTestData { value = 12 });
            var entityB = CreateEntity(new EcsTestData { value = 22 });
            m_World.EntityManager.AddSharedComponentData(entityA, new EcsTestSharedComp { value = 1 });
            m_World.EntityManager.AddSharedComponentData(entityB, new EcsTestSharedComp { value = 1 });
            var entityC = CreateEntity(new EcsTestData { value = 32 });
            m_World.EntityManager.AddSharedComponentData(entityC, new EcsTestSharedComp { value = 2 });

            using (m_ChunkDiffer.GatherComponentChangesAsync(m_World.EntityManager.UniversalQuery, Allocator.TempJob, out var jobHandle))
            {
                jobHandle.Complete();
            }

            m_World.EntityManager.GetCheckedEntityDataAccess()->EntityComponentStore->IncrementGlobalSystemVersion();
            m_World.EntityManager.RemoveComponent<EcsTestData>(entityB);
            Assert.That(m_World.EntityManager.HasComponent<EcsTestData>(entityB), Is.False);
            var entityBbis = CreateEntity(new EcsTestData { value = 52 });
            m_World.EntityManager.AddSharedComponentData(entityBbis, new EcsTestSharedComp { value = 1 });
            var entityD = CreateEntity(new EcsTestData { value = 42 });
            m_World.EntityManager.AddSharedComponentData(entityD, new EcsTestSharedComp { value = 2 });

            using (var result = m_ChunkDiffer.GatherComponentChangesAsync(m_World.EntityManager.UniversalQuery, Allocator.TempJob, out var jobHandle))
            {
                jobHandle.Complete();
                Assert.That(result.AddedComponentsCount, Is.EqualTo(2));
                Assert.That(result.RemovedComponentsCount, Is.EqualTo(1));

                Assert.That(result.GetAddedComponent<EcsTestData>(0), Is.EqualTo((entityBbis, new EcsTestData { value = 52 })));
                Assert.That(result.GetAddedComponent<EcsTestData>(1), Is.EqualTo((entityD, new EcsTestData { value = 42 })));
                Assert.That(result.GetRemovedComponent<EcsTestData>(0), Is.EqualTo((entityB, new EcsTestData { value = 22 })));
            }
        }

        [Test]
        public unsafe void ComponentDataDiffer_DetectMissingChunk()
        {
            var entityA = CreateEntity(new EcsTestData { value = 12 });
            var entityInChunk = m_World.EntityManager.GetCheckedEntityDataAccess()->EntityComponentStore->GetEntityInChunk(entityA);
            Assert.That(entityInChunk.Chunk != null);

            using (m_ChunkDiffer.GatherComponentChangesAsync(m_World.EntityManager.UniversalQuery, Allocator.TempJob, out var jobHandle))
            {
                jobHandle.Complete();
            }

            m_World.EntityManager.GetCheckedEntityDataAccess()->EntityComponentStore->IncrementGlobalSystemVersion();
            m_World.EntityManager.DestroyEntity(entityA);
            entityInChunk = m_World.EntityManager.GetCheckedEntityDataAccess()->EntityComponentStore->GetEntityInChunk(entityA);

            Assert.That(entityInChunk.Chunk == null);

            using (var result = m_ChunkDiffer.GatherComponentChangesAsync(m_World.EntityManager.UniversalQuery, Allocator.TempJob, out var jobHandle))
            {
                jobHandle.Complete();
                Assert.That(result.AddedComponentsCount, Is.EqualTo(0));
                Assert.That(result.RemovedComponentsCount, Is.EqualTo(1));

                Assert.That(result.GetRemovedComponent<EcsTestData>(0), Is.EqualTo((entityA, new EcsTestData { value = 12 })));
            }
        }

        [Test]
        public void ComponentDataDiffer_DetectChangingQuery()
        {
            using (var customQuery = m_World.EntityManager.CreateEntityQuery(typeof(EcsTestData), typeof(EcsTestData2)))
            {
                var entityA = CreateEntity(new EcsTestData { value = 12 });
                var entityB = CreateEntity(new EcsTestData { value = 22 });
                m_World.EntityManager.AddComponentData(entityB, new EcsTestData2 { value0 = 32, value1 = 42 });

                using (var result = m_ChunkDiffer.GatherComponentChangesAsync(customQuery, Allocator.TempJob, out var jobHandle))
                {
                    jobHandle.Complete();
                    Assert.That(result.AddedComponentsCount, Is.EqualTo(1));
                    Assert.That(result.RemovedComponentsCount, Is.EqualTo(0));

                    Assert.That(result.GetAddedComponent<EcsTestData>(0), Is.EqualTo((entityB, new EcsTestData { value = 22 })));
                }

                using (var result = m_ChunkDiffer.GatherComponentChangesAsync(m_World.EntityManager.UniversalQuery, Allocator.TempJob, out var jobHandle))
                {
                    jobHandle.Complete();
                    Assert.That(result.AddedComponentsCount, Is.EqualTo(1));
                    Assert.That(result.RemovedComponentsCount, Is.EqualTo(0));

                    Assert.That(result.GetAddedComponent<EcsTestData>(0), Is.EqualTo((entityA, new EcsTestData { value = 12 })));
                }

                {
                    var result = m_ChunkDiffer.GatherComponentChangesAsync(customQuery, Allocator.TempJob, out var jobHandle);
                    jobHandle.Complete();
                    Assert.That(result.AddedComponentsCount, Is.EqualTo(0));
                    Assert.That(result.RemovedComponentsCount, Is.EqualTo(1));

                    Assert.That(result.GetRemovedComponent<EcsTestData>(0), Is.EqualTo((entityA, new EcsTestData { value = 12 })));
                    result.Dispose();
                }
            }
        }

        [Test]
        public unsafe void ComponentDataDiffer_MovedEntityWithinChunkDetectedAsRemovedAndAdded()
        {
            m_World.EntityManager.GetCheckedEntityDataAccess()->EntityComponentStore->IncrementGlobalSystemVersion();
            var entity1 = CreateEntity(new EcsTestData { value = 1 });
            var entity2 = CreateEntity(new EcsTestData { value = 2 });
            using (m_ChunkDiffer.GatherComponentChangesAsync(m_World.EntityManager.UniversalQuery, Allocator.TempJob, out var jobHandle))
            {
                jobHandle.Complete();
            }

            m_World.EntityManager.GetCheckedEntityDataAccess()->EntityComponentStore->IncrementGlobalSystemVersion();
            m_World.EntityManager.DestroyEntity(entity1);
            using (var result = m_ChunkDiffer.GatherComponentChangesAsync(m_World.EntityManager.UniversalQuery, Allocator.TempJob, out var jobHandle))
            {
                jobHandle.Complete();

                Assert.That(result.RemovedComponentsCount, Is.EqualTo(2));
                Assert.That(result.AddedComponentsCount, Is.EqualTo(1));

                // entity1 is detected as removed because it's been deleted
                Assert.That(result.GetRemovedComponent<EcsTestData>(0), Is.EqualTo((entity1, new EcsTestData { value = 1 })));
                // entity2 is detected as removed because it was removed from position 1
                Assert.That(result.GetRemovedComponent<EcsTestData>(1), Is.EqualTo((entity2, new EcsTestData { value = 2 })));
                // entity2 is detected as re-added because it was added at position 0
                Assert.That(result.GetAddedComponent<EcsTestData>(0), Is.EqualTo((entity2, new EcsTestData { value = 2 })));
            }
        }

        [Test]
        public unsafe void ComponentDataDiffer_MovedEntityBetweenChunksDetectedAsRemovedAndAdded()
        {
            m_World.EntityManager.GetCheckedEntityDataAccess()->EntityComponentStore->IncrementGlobalSystemVersion();

            var archetypeA = m_World.EntityManager.CreateArchetype(typeof(EcsTestData), typeof(CompA));
            var archetypeB = m_World.EntityManager.CreateArchetype(typeof(EcsTestData), typeof(CompB));
            var entityA1 = m_World.EntityManager.CreateEntity(archetypeA);
            var entityA2 = m_World.EntityManager.CreateEntity(archetypeA);
            var entityB1 = m_World.EntityManager.CreateEntity(archetypeB);
            m_World.EntityManager.SetComponentData(entityA1, new EcsTestData { value = 1 });
            m_World.EntityManager.SetComponentData(entityA2, new EcsTestData { value = 2 });
            m_World.EntityManager.SetComponentData(entityB1, new EcsTestData { value = 1 });
            using (m_ChunkDiffer.GatherComponentChangesAsync(m_World.EntityManager.UniversalQuery, Allocator.TempJob, out var jobHandle))
            {
                jobHandle.Complete();
            }

            m_World.EntityManager.GetCheckedEntityDataAccess()->EntityComponentStore->IncrementGlobalSystemVersion();
            // Move entityA2 from archetype A to archetype B to provoke a change of chunk
            m_World.EntityManager.RemoveComponent<CompA>(entityA2);
            m_World.EntityManager.AddComponent<CompB>(entityA2);

            using (var result = m_ChunkDiffer.GatherComponentChangesAsync(m_World.EntityManager.UniversalQuery, Allocator.TempJob, out var jobHandle))
            {
                jobHandle.Complete();
                Assert.That(result.RemovedComponentsCount, Is.EqualTo(1));
                Assert.That(result.AddedComponentsCount, Is.EqualTo(1));

                Assert.That(result.GetRemovedComponent<EcsTestData>(0), Is.EqualTo((entityA2, new EcsTestData { value = 2 })));
                Assert.That(result.GetAddedComponent<EcsTestData>(0), Is.EqualTo((entityA2, new EcsTestData { value = 2 })));
            }
        }

        [Test]
        public unsafe void ComponentDataDiffer_DetectNewEntityWithSameComponentData()
        {
            m_World.EntityManager.GetCheckedEntityDataAccess()->EntityComponentStore->IncrementGlobalSystemVersion();
            var entityA = CreateEntity(new EcsTestData { value = 1 });
            var entityB = CreateEntity(new EcsTestData { value = 1 });
            using (m_ChunkDiffer.GatherComponentChangesAsync(m_World.EntityManager.UniversalQuery, Allocator.TempJob, out var jobHandle))
            {
                jobHandle.Complete();
            }

            m_World.EntityManager.GetCheckedEntityDataAccess()->EntityComponentStore->IncrementGlobalSystemVersion();
            m_World.EntityManager.DestroyEntity(entityB);
            var entityC = CreateEntity(new EcsTestData { value = 1 });
            using (var result = m_ChunkDiffer.GatherComponentChangesAsync(m_World.EntityManager.UniversalQuery, Allocator.TempJob, out var jobHandle))
            {
                jobHandle.Complete();
                Assert.That(result.AddedComponentsCount, Is.EqualTo(1));
                Assert.That(result.RemovedComponentsCount, Is.EqualTo(1));

                Assert.That(result.GetRemovedComponent<EcsTestData>(0), Is.EqualTo((entityB, new EcsTestData { value = 1 })));
                Assert.That(result.GetAddedComponent<EcsTestData>(0), Is.EqualTo((entityC, new EcsTestData { value = 1 })));
            }
        }

        [Test]
        public void ComponentDataDiffer_CheckIfDifferCanWatchType()
        {
            Assert.That(ComponentDataDiffer.CanWatch(typeof(EcsTestData)), Is.True);
            Assert.That(ComponentDataDiffer.CanWatch(typeof(EcsTestSharedComp)), Is.False);
            Assert.That(ComponentDataDiffer.CanWatch(typeof(Entity)), Is.False);
        }

        struct CompA : IComponentData
        {
#pragma warning disable 649
            public int Value;
#pragma warning restore 649
        }

        struct CompB : IComponentData
        {
#pragma warning disable 649
            public int Value;
#pragma warning restore 649
        }

        Entity CreateEntity<T>(T data) where T : struct, IComponentData
        {
            var e = m_World.EntityManager.CreateEntity();
            m_World.EntityManager.AddComponentData(e, data);

            return e;
        }
    }
}
