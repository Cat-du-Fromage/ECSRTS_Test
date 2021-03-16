using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

/*
//[UpdateAfter(typeof(UnitsSystem))]
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
            .WithName("HIGHLIGHTDISABLE")
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
*/
//=================================================================================================================================
/// <summary>
/// ENABLE Selection Highlight
/// Dependency: HoverSystem.cs
/// </summary>
//=================================================================================================================================
public class EnableHighlight : SystemBase
{
    BeginInitializationEntityCommandBufferSystem ECB_bSim;
    private EntityQuery _test;
    protected override void OnCreate()
    {
        ECB_bSim = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        RequireForUpdate(GetEntityQuery(typeof(RegimentUnitSelectedTag)));
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer.ParallelWriter ecbBsim = ECB_bSim.CreateCommandBuffer().AsParallelWriter();
        UnityEngine.Debug.Log("HighlightEnable");
        Entities
            .WithAll<RegimentTag, RegimentUnitSelectedTag>()
            .WithName("HIGHLIGHTenable")
            .WithBurst()
            .ForEach((Entity RegimentSelected, int entityInQueryIndex, in DynamicBuffer<RegimentHighlightsBuffer> highlights) =>
            {
                NativeArray<Entity> highlightsReg = highlights.Reinterpret<Entity>().ToNativeArray(Allocator.Temp);
                for (int i = 0; i < highlightsReg.Length; i++)
                {
                    if (HasComponent<Disabled>(highlightsReg[i]))
                    {
                        ecbBsim.RemoveComponent<Disabled>(entityInQueryIndex, highlightsReg[i]);
                    }
                }
                highlightsReg.Dispose();
                ecbBsim.AddComponent<RegimentSelectedTag>(entityInQueryIndex, RegimentSelected);
                ecbBsim.RemoveComponent<RegimentUnitSelectedTag>(entityInQueryIndex, RegimentSelected);
            }).Schedule();
        ECB_bSim.AddJobHandleForProducer(this.Dependency);
    }
}

//=================================================================================================================================
/// <summary>
/// DISABLE Selection Highlight
/// Dependency: HoverSystem.cs
/// </summary>
//=================================================================================================================================
public class HighlightDisable : SystemBase
{
    BeginInitializationEntityCommandBufferSystem ECB_bSim;
    protected override void OnCreate()
    {
        ECB_bSim = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        RequireForUpdate(GetEntityQuery(typeof(RegimentDeselect)));
    }
    protected override void OnUpdate()
    {
        EntityCommandBuffer.ParallelWriter ecbBsim = ECB_bSim.CreateCommandBuffer().AsParallelWriter(); // done at the begining

        UnityEngine.Debug.Log("HighlightDnable");
        Entities
            .WithName("HIGHLIGHTDISABLE")
            .WithAll<RegimentDeselect>()
            .WithBurst()
            .ForEach((Entity regimentDeselect, int entityInQueryIndex, in DynamicBuffer<RegimentHighlightsBuffer> highlights) =>
            {
                NativeArray<Entity> highlightsReg = highlights.Reinterpret<Entity>().ToNativeArray(Allocator.Temp);
                for (int i = 0; i < highlightsReg.Length; i++)
                {
                    if (!HasComponent<Disabled>(highlightsReg[i]))
                    {
                        ecbBsim.AddComponent<Disabled>(entityInQueryIndex, highlightsReg[i]);
                    }
                }
                highlightsReg.Dispose();
                ecbBsim.RemoveComponent<RegimentSelectedTag>(entityInQueryIndex, regimentDeselect);
                UnityEngine.Debug.Log("REMOVE OK");
                ecbBsim.RemoveComponent<RegimentDeselect>(entityInQueryIndex, regimentDeselect);
            }).Schedule();
        ECB_bSim.AddJobHandleForProducer(Dependency);
    }
}



