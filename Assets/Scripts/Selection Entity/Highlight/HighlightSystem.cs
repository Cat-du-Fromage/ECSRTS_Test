using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

/// <summary>
/// Disable all Unit Selection on creation of Units
/// </summary>
//[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(UnitsSystem))]
public class HighlightSystem : SystemBase
{
    BeginInitializationEntityCommandBufferSystem ECB_bSim;

    protected override void OnCreate()
    {
        base.OnCreate();
        ECB_bSim = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }
    protected override void OnStartRunning()
    {
        EntityCommandBuffer.ParallelWriter ecbBsim = ECB_bSim.CreateCommandBuffer().AsParallelWriter(); // done at the begining

        UnityEngine.Debug.Log("HighlightDnable");
        Entities
            .WithAll<UnitTag>()
            .WithNone<SelectedUnitTag, UnitNoNeedHighlightTag, UnitNeedHighlightTag>()
            .WithBurst()
            //.WithEntityQueryOptions(EntityQueryOptions.IncludeDisabled)
            .ForEach((Entity UnitSelected, int entityInQueryIndex, in DynamicBuffer<LinkedEntityGroup> linkedEntity) =>
            {
                for (int i = 1; i < linkedEntity.Length; i++)
                {
                    if (HasComponent<HighlightTag>(linkedEntity[i].Value))
                    {
                        ecbBsim.AddComponent<Disabled>(entityInQueryIndex, linkedEntity[i].Value);
                    }
                }
                //ecbBsim.RemoveComponent<UnitNeedHighlightTag>(entityInQueryIndex, UnitSelected);
            }).Schedule();
        ECB_bSim.AddJobHandleForProducer(Dependency);
    }
    protected override void OnUpdate()
    {

    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
}



