using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class EnableHighlight : SystemBase
{
    BeginInitializationEntityCommandBufferSystem ECB_bSim;

    protected override void OnCreate()
    {
        ECB_bSim = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        RequireSingletonForUpdate<UnitNeedHighlightTag>();
        Debug.Log("Enable Highlight ENTER");
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer.ParallelWriter ecbBsim = ECB_bSim.CreateCommandBuffer().AsParallelWriter();
        UnityEngine.Debug.Log("HighlightEnable");
        Entities
            .WithAll<SelectedUnitTag, UnitNeedHighlightTag, UnitTag>()
            .WithBurst()
            .ForEach((Entity UnitSelected, int entityInQueryIndex, in DynamicBuffer<LinkedEntityGroup> linkedEntity) =>
            {
                //UnityEngine.Debug.Log("Pass1");
                for (int i = 1; i < linkedEntity.Length; i++)
                {
                    //UnityEngine.Debug.Log("Pass2");
                    if (HasComponent<HighlightTag>(linkedEntity[i].Value))
                    {
                        //UnityEngine.Debug.Log("HighlightEnable Pass3");
                        ecbBsim.RemoveComponent<Disabled>(entityInQueryIndex, linkedEntity[i].Value);
                    }
                }
                ecbBsim.RemoveComponent<UnitNeedHighlightTag>(entityInQueryIndex, UnitSelected);
            }).ScheduleParallel();
        ECB_bSim.AddJobHandleForProducer(this.Dependency);
    }

    protected override void OnStopRunning()
    {
        base.OnStopRunning();
        Debug.Log("Enable Highlight EXIT");
    }
}


