using System.Diagnostics;
using System.IO;
using System.Text;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.Physics;
using Unity.Rendering;
using UnityEngine;
using Application = UnityEngine.Application;
using Debug = UnityEngine.Debug;

public class SaveManager : MonoBehaviour
{
    private EntityQuery entitiesToSaveQuery;
    public static SaveManager instance;

    [UnityEngine.ContextMenu("Save")]
    public void TestSave()
    {
        Save(Application.persistentDataPath);
        OpenPath(Application.persistentDataPath);
    }
    
    [UnityEngine.ContextMenu("Load")]
    public void TestLoad()
    {
        Load(Application.persistentDataPath);
    }

    [UnityEngine.ContextMenu("Destroy")]
    public void TestDestroy()
    {
        //World.DefaultGameObjectInjectionWorld.EntityManager.UniversalQuery
        World.DefaultGameObjectInjectionWorld.EntityManager.DestroyEntity(entitiesToSaveQuery);
    }
    
    void OpenPath(string path)
    {
        var myString = path.Replace(@"/","\\");
        //var path = System.IO.Directory.GetCurrentDirectory() + "\\" + "Release" + "\\";
        if (Directory.Exists(path))
        {
            var processStartInfo = new ProcessStartInfo
            {
                Arguments = myString,
                FileName  = "explorer.exe"
            };

            Process.Start(processStartInfo);
        }
    }
    
    private void Start()
    {
        // Cache a query that gathers all of the entities that should be saved.
        // NOTE: You don't have to use a special tag component for all entities you want to save. You could instead just
        // save, for example, anything with a Translation component which would exclude things like Singletons entities.
        // It is important to note that prefabs (anything with a Prefab tag component) are automatically excluded from
        // an EntityQuery unless EntityQueryOptions.IncludePrefab is set.
        var savableEntities = new EntityQueryDesc
        {
            Any = new ComponentType[]
            {
                typeof(SaveEntity),
            },
            Options = EntityQueryOptions.Default
        };
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        entitiesToSaveQuery = entityManager.CreateEntityQuery(savableEntities);

        instance = this;
    }

    // Looks for and removes a set of components and then adds a different set of components to the same set
    // of entities. 
    private static void ReplaceComponents(ComponentType[] typesToRemove, ComponentType[] typesToAdd, EntityManager entityManager)
    {
        var query = entityManager.CreateEntityQuery(
            new EntityQueryDesc { Any = typesToRemove, Options = EntityQueryOptions.Default }
        );
        var entities = query.ToEntityArray(Allocator.Temp);

        foreach (ComponentType removeType in typesToRemove)
        {
            entityManager.RemoveComponent(entities, removeType);
        }
        foreach (ComponentType addType in typesToAdd)
        {
            entityManager.AddComponent(entities, addType);
        }
    }

    public void Save(string path)
    {
        /*
         * 1. Create a new world.
         * 2. Copy over the entities we want to serialize to the new world.
         * 3. Remove all shared components, components containing blob asset references, and components containing
         *    external entity references.
         * 4. Serialize the new world to a save file.
         */
        
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        //var serializeWorld = new World("Serialization World");
        using (var serializeWorld = new World("Saving World"))
        {
            var serializeManager = serializeWorld.EntityManager;
            serializeManager.CopyEntitiesFrom(entityManager, entitiesToSaveQuery.ToEntityArray(Allocator.Temp));

            // Remove RenderMesh and related components
            ReplaceComponents(
                new ComponentType[]
                {
                    typeof(RenderMesh),
                    typeof(EditorRenderData),
                    typeof(WorldRenderBounds),
                    typeof(ChunkWorldRenderBounds),
                    typeof(HybridChunkInfo),
                    typeof(RenderBounds)
                },
                new ComponentType[] { typeof(RenderMeshRemoved) },
                serializeManager
            );

            // Remove physics colliders
            ReplaceComponents(
                new ComponentType[]
                {
                    typeof(PhysicsCollider),
                },
                new ComponentType[] { typeof(PhysicsColliderRemoved) },
                serializeManager
            );

            // Remove blob assets.
            ReplaceComponents(
                new ComponentType[]
                {
                    typeof(MyBlobComponent),
                },
                new ComponentType[] { typeof(MissingMyBlobComponentTag) },
                serializeManager
            );
            
            // Need to remove the SceneTag shared component from all entities because it contains an entity reference
            // that exists outside the subscene which isn't allowed for SerializeUtility. This breaks the link from the
            // entity to the subscene, but otherwise doesn't seem to cause any problems.
            serializeManager.RemoveComponent<SceneTag>(serializeManager.UniversalQuery);
            serializeManager.RemoveComponent<SceneSection>(serializeManager.UniversalQuery);

            var binary =  path + "\\" + "DefaultWorld.world"; // path backslash for system access
            var yaml = path + "\\" + "DefaultWorld.yaml";
            // Save
            using (var writer = new StreamBinaryWriter(binary))
            {
                SerializeUtility.SerializeWorld(serializeManager, writer);
            }
            
            using (var writer = new StreamWriter(yaml))
            {
                writer.NewLine = "\n";
                SerializeUtility.SerializeWorldIntoYAML(serializeManager, writer, false);
            }
        }
    }

    public void Load(string filepath)
    {
        var binary =  filepath + "\\" + "DefaultWorld.world"; // path backslash for system access
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        entityManager.DestroyEntity(entitiesToSaveQuery);

        using (var deserializeWorld = new World("Deserialization World"))
        {
            ExclusiveEntityTransaction transaction = deserializeWorld.EntityManager.BeginExclusiveEntityTransaction();

            using (var reader = new StreamBinaryReader(binary))
            {
                SerializeUtility.DeserializeWorld(transaction, reader);
            }

            deserializeWorld.EntityManager.EndExclusiveEntityTransaction();

            entityManager.MoveEntitiesFrom(deserializeWorld.EntityManager);
        }
    }
}