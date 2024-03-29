using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public class RegimentPlacementSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem ESim_ecb;
    protected override void OnCreate()
    {
        base.OnCreate();
        //this.Enabled = false;
        RequireSingletonForUpdate<Trigger_Placement_Regiment>();
        ESim_ecb = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
        /*
        var description = new EntityQueryDesc()
        {
            All = new ComponentType[]
                   {
                        ComponentType.ReadOnly<State_Selected>(),
                        ComponentType.ReadOnly<RegimentTag>(),
                        ComponentType.ReadOnly<CompRegimentClass_Fusilier>()
                    }
        };
        EntityQuery query = GetEntityQuery(description);
        */
    }
    protected override void OnUpdate()
    {
        EntityCommandBuffer.ParallelWriter ecb = ESim_ecb.CreateCommandBuffer().AsParallelWriter();

        //NativeArray<float> randomNumbers = new NativeArray<float>(10, Allocator.TempJob);
        var description = new EntityQueryDesc()
        {
            All = new ComponentType[]
                   {
                        ComponentType.ReadOnly<State_Selected>(),
                        ComponentType.ReadOnly<RegimentTag>(),
                        ComponentType.ReadOnly<CompRegimentClass_Fusilier>()
                    }
        };
        EntityQuery query = GetEntityQuery(description);
        //CAREFUL: Selected Regiment are only indicators(number seperation(nbregiment) AND NumberUnit(perRegiment))
        Entities
        .WithAll<Trigger_Placement_Regiment>()
        .ForEach((Entity regiment, int entityInQueryIndex ,ref Translation translation, in Rotation rotation) => 
        {
            // Implement the work to perform for each entity here.
            // You should only access data that is local or that is a
            // field on this job. Note that the 'rotation' parameter is
            // marked as 'in', which means it cannot be modified,
            // but allows this job to run in parallel with other jobs
            // that want to read Rotation component data.
            // For example,
            //     translation.Value += math.mul(rotation.Value, new float3(0, 0, 1)) * deltaTime;
            ecb.RemoveComponent<Trigger_Placement_Regiment>(entityInQueryIndex, regiment);

        }).Schedule();
        ESim_ecb.AddJobHandleForProducer(Dependency);
        //randomNumbers.Dispose();
    }

    struct TestJob : IJob
    {
        public NativeArray<Entity> regimentSelected;
        public ComponentTypeHandle<CompRegimentClass_Fusilier> RegSize;
        public void Execute()
        {
            
        }
    }
}
