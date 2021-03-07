using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
/// <summary>
/// Disable all Unit Selection on creation of Units
/// </summary>
//[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(UnitsSystem))]
public class HighlightSystem : SystemBase
{
    BeginInitializationEntityCommandBufferSystem ECB_bSim;

    protected override void OnCreate()
    {
        ECB_bSim = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }
    protected override void OnStartRunning()
    {
        EntityCommandBuffer.ParallelWriter ecbBsim = ECB_bSim.CreateCommandBuffer().AsParallelWriter(); // done at the begining

        UnityEngine.Debug.Log("HighlightDnable");
        Entities
            .WithAll<UnitTag>()
            .WithNone<SelectedUnitTag>()
            .WithBurst()
            //.WithEntityQueryOptions(EntityQueryOptions.IncludeDisabled)
            .ForEach((Entity UnitSelected, int entityInQueryIndex, in DynamicBuffer<LinkedEntityGroup> linkedEntity) =>
            {
                UnityEngine.Debug.Log("DPass1");
                for (int i = 1; i < linkedEntity.Length; i++)
                {
                    UnityEngine.Debug.Log("DPass2");
                    if (HasComponent<HighlightTag>(linkedEntity[i].Value))
                    {
                        UnityEngine.Debug.Log("HighlightDnable Pass3");
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
}


