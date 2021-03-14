using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Rendering;



//=================================================================================================================================
/// <summary>
/// ENABLE PRESELECT HIGHLIGHTS
/// Dependency: HoverSystem.cs
/// </summary>
//=================================================================================================================================
public class HoverHighligntEnable : SystemBase
{
    BeginInitializationEntityCommandBufferSystem ECB_bInit;

    /// <summary>
    /// WAIT for the component EnterHoverTag
    /// </summary>
    protected override void OnCreate()
    {
        base.OnCreate();
        RequireForUpdate(GetEntityQuery(typeof(EnterHoverTag)));
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
//=================================================================================================================================
/// <summary>
/// DISABLE Pre-Selection for hovering
/// Dependency: HoverSystem.cs
/// </summary>
//=================================================================================================================================
public class HoverHighligntDisable : SystemBase
{
    BeginInitializationEntityCommandBufferSystem ECB_bInit;
    /// <summary>
    /// WAIT for the component ExitHoverTag
    /// </summary>
    protected override void OnCreate()
    {
        base.OnCreate();
        RequireForUpdate(GetEntityQuery(typeof(ExitHoverTag)));
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
                ecb.RemoveComponent<HoverTag>(entityInQueryIndex, regiment); //remove tag on 
            }).ScheduleParallel();
        ECB_bInit.AddJobHandleForProducer(Dependency);
    }
}
