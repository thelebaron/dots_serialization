using System.IO;
using Unity.Entities;
using Unity.Entities.Serialization;
using UnityEditor;
using UnityEngine;

namespace DefaultNamespace
{
    public static class WorldManagement
    {
        private static string WorldLocation = Path.SaveDir + "\\" + "DefaultWorld.world";
        private static string WorldReferencesLocation = Path.SaveDir + "\\" + "DefaultWorldReferences.asset";
        
        /// <summary>
        /// Default save method. Saves world as binary file.
        /// </summary>
        /// <param name="debug"></param>
        public static void Save(bool debug = false)
        {
            if(World.All.Count<1)
                return;

            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            // Path for saving world
            var binaryWriter = new StreamBinaryWriter(WorldLocation);
                    
            // Save whole world
            // pure version - SerializeUtility.SerializeWorld(em, binaryWriter, out var objectReferences);
            SerializeUtilityHybrid.Serialize(entityManager, binaryWriter, out var objectReferences);
            
            if(debug)
                QueryReferences.Query(objectReferences);
            
            AssetDatabase.CreateAsset(objectReferences, WorldReferencesLocation);
            Debug.Log("Saved");
        }

        public static void Load()
        {
            if(World.All.Count<1)
                return;
            
            // need an empty world to do this
            var world       = new World("LoadingWorld");
            
            using (var reader = new StreamBinaryReader(WorldLocation)) //GetFullPathByName(fileName))
            {
                var objectRefAsset = AssetDatabase.LoadAssetAtPath<ReferencedUnityObjects>(WorldReferencesLocation);
                // Load objects as binary file
                SerializeUtilityHybrid.Deserialize(world.EntityManager, reader, objectRefAsset);
            }
            
            World.DefaultGameObjectInjectionWorld.EntityManager.DestroyEntity(World.DefaultGameObjectInjectionWorld.EntityManager.UniversalQuery);
            World.DefaultGameObjectInjectionWorld.EntityManager.MoveEntitiesFrom(world.EntityManager);
            Debug.Log("Loaded");
        }
        
        /// <summary>
        /// Testing yaml save
        /// </summary>
        public static void SaveYAML()
        {
            if(World.All.Count<1)
                return;
                
            var refFilePathName = "";
            
            // Save world
            using (var writer = new StreamWriter(refFilePathName))
            {
                writer.NewLine = "\n";
                var em = World.DefaultGameObjectInjectionWorld.EntityManager;
                // Save world to yaml
                SerializeUtility.SerializeWorldIntoYAML(em, writer, false); // is yaml just debugging?
                    
            }
        }
    }

    public static class Path
    {
        public static string FileLocation => Application.dataPath;
        
        public const string ProjectName = "Cyberjunk";
        public const string SaveDir = "Assets/Saved";
        
    }
}