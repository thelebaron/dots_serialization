using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Random = UnityEngine.Random;

public class DummyMoveSystem : SystemBase
{
    public EntityQuery query;
    protected override void OnCreate()
    {
        base.OnCreate();
        query = GetEntityQuery(ComponentType.Exclude<RandomDirection>(), ComponentType.ReadWrite<Translation>());
    }

    protected override void OnUpdate()
    {
        // add random component
        if (query.CalculateEntityCount() != 0)
        {
            var native = query.ToEntityArray(Allocator.Temp);
            for (int i = 0; i < native.Length; i++)
            {
                var entity = native[i];
                EntityManager.AddComponentData(entity, new RandomDirection {Direction = new float3(Random.Range(-1,1f), Random.Range(-1,1f), Random.Range(-1,1f))});
                
            }

            native.Dispose();
        }
        float time = Time.DeltaTime;
        Entities.ForEach((Entity entity, ref Translation translation, in RandomDirection randomDirection) =>
        {
            //translation.Value += time * randomDirection.Direction * 2;
            
        }).Schedule();
        
    }

    public struct RandomDirection : IComponentData
    {
        public float3 Direction;
    }
}
