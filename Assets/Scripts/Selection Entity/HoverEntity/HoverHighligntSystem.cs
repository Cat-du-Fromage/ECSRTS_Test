using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
/// <summary>
/// ENABLE PRESELECT HIGHLIGHTS
/// </summary>
public class HoverHighligntEnable : SystemBase
{
    private EntityQuery _OnEnterHover;
    BeginInitializationEntityCommandBufferSystem ECB_bInit;
    protected override void OnCreate()
    {
        base.OnCreate();
        this._OnEnterHover = GetEntityQuery(typeof(EnterHoverTag)); //if the entity query is empty, the system won't update
        ECB_bInit = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer.ParallelWriter ecb = ECB_bInit.CreateCommandBuffer().AsParallelWriter();
        Entities
            .WithAll<RegimentTag, EnterHoverTag>()
            .WithBurst()
            .ForEach((Entity regiment, int entityInQueryIndex, in DynamicBuffer<RegimentPreSelectBuffer> unitPreselect) =>
            {
                NativeArray<RegimentPreSelectBuffer> RegimentHighlights = unitPreselect.ToNativeArray(Allocator.Temp);
                for (int i = 0; i< RegimentHighlights.Length; i++)
                {
                    ecb.RemoveComponent<Disabled>(entityInQueryIndex, RegimentHighlights[i].UnitPreselect);
                }
                RegimentHighlights.Dispose();
                ecb.RemoveComponent<EnterHoverTag>(entityInQueryIndex, regiment);
            }).ScheduleParallel();
        ECB_bInit.AddJobHandleForProducer(Dependency);
    }
}

public class HoverHighligntDisable : SystemBase
{
    private EntityQuery _OnExitHover;
    BeginInitializationEntityCommandBufferSystem ECB_bInit;
    protected override void OnCreate()
    {
        base.OnCreate();
        this._OnExitHover = GetEntityQuery(typeof(ExitHoverTag)); //if the entity query is empty, the system won't update
        ECB_bInit = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer.ParallelWriter ecb = ECB_bInit.CreateCommandBuffer().AsParallelWriter();
        Entities
            .WithAll<RegimentTag, ExitHoverTag>()
            .WithBurst()
            .ForEach((Entity regiment, int entityInQueryIndex, in DynamicBuffer<RegimentPreSelectBuffer> unitPreselect) =>
            {
                NativeArray<RegimentPreSelectBuffer> RegimentHighlights = unitPreselect.ToNativeArray(Allocator.Temp);
                for (int i = 0; i < RegimentHighlights.Length; i++)
                {
                    ecb.AddComponent<Disabled>(entityInQueryIndex, RegimentHighlights[i].UnitPreselect);
                }
                RegimentHighlights.Dispose();
                ecb.RemoveComponent<ExitHoverTag>(entityInQueryIndex, regiment);
            }).ScheduleParallel();
        ECB_bInit.AddJobHandleForProducer(Dependency);
    }
}
