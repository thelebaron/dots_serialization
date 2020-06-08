using Unity.Entities;
using Unity.Entities.Serialization;

namespace DefaultNamespace
{
    public class EntitiesWorldSerialization
    {
        /*public void SerializeWorld(SaveData saveData) 
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            int[]         sc;
            
            using (var writer = new StreamBinaryWriter(saveData.name)) 
            {
                //SerializeUtility.SerializeWorld(entityManager, writer, out sc);
            }
            using (var writer = new StreamBinaryWriter(saveData.name+"_shared")) 
            {
                //SerializeUtility.SerializeSharedComponents(entityManager, writer, sc);
                
                //SerializeUtility.SerializeWorld(entityManager, writer, sc);
                //SerializeUtility.SerializeWorldIntoYAML(entityManager, writer, true);
            }
        }*/
     /*
        public void DeserializeWorld(SaveData saveData) {
            World localWorld = new World("local world");
            int   num;
            using (var reader = new StreamBinaryReader(saveData.name + "_shared")) {
                num = SerializeUtility.DeserializeSharedComponents(localWorld.EntityManager, reader);
            }
     
            var transaction = localWorld.EntityManager.BeginExclusiveEntityTransaction();
            using (var reader = new StreamBinaryReader(saveData.name)) {
                SerializeUtility.DeserializeWorld(transaction, reader, num);
            }
            SerializeUtilityHybrid.ReleaseSharedComponents(transaction, num);
            localWorld.EntityManager.EndExclusiveEntityTransaction();
            World.Active.EntityManager.MoveEntitiesFrom(localWorld.EntityManager);
        }*/

    }
}