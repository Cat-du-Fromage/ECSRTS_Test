using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Rendering;
using Unity.Physics;
//[AlwaysUpdateSystem]
//[UpdateAfter(typeof(SelectionSystem))]
//[UpdateInGroup(typeof(SelectionSystem))]
public class RegimentsSystem : SystemBase
{
    public enum UnitFusilier
    {
        PrussianFusilier,
        FrenchFusilier,
        BritishFusilier
    };

    public enum RegimentType
    {
        Fusilier,
        Cavalry,
        artillerie
    };

    private EntityManager _entityManager;
    private EntityArchetype _regimentFusilierArchetype;
    protected override void OnCreate()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        _regimentFusilierArchetype = _entityManager.CreateArchetype(typeof(RegimentTag),typeof(RegimentUnassignedTag),typeof(LocalToWorld),typeof(RegimentHighlightsBuffer),typeof(RegimentPreSelectBuffer),typeof(Translation),typeof(RenderBounds), typeof(CompRegimentClass_Fusilier)); // set the archetype of a new Regiment
    }

    protected override void OnUpdate()
    {
        //Simulation of the spawning of regiment(like in total war)

        //Improvement to make:
        //1)register Units for each click
        //2) when press "enter" spawn all 
        if (Input.GetKeyDown(KeyCode.L))
        {
            CreateRegiment(UnitFusilier.PrussianFusilier, RegimentType.Fusilier);
        }
        else if (Input.GetKeyDown(KeyCode.K))
        {
            CreateRegiment(UnitFusilier.BritishFusilier, RegimentType.Fusilier);
        }
    }

    public void CreateRegiment(UnitFusilier unitType, RegimentType regimentType)
    {
        EntityQuery spawnerQ = GetEntityQuery(typeof(SpawnerUnitsTag));
        Entity spawner = spawnerQ.GetSingletonEntity();
        Spawn_UnitFusilier spawnerPrefab = _entityManager.GetComponentData<Spawn_UnitFusilier>(spawner);

        Entity RegimentFusilier = _entityManager.CreateEntity(_regimentFusilierArchetype);
        _entityManager.SetComponentData(RegimentFusilier, new CompRegimentClass_Fusilier { Size = 10 });//find a way to assign a number to a class Component
        if (unitType == UnitFusilier.PrussianFusilier)
        {
            _entityManager.AddComponent<UnitType_Prefab>(RegimentFusilier);
            _entityManager.SetComponentData(RegimentFusilier, new UnitType_Prefab { UnitTypePrefab = spawnerPrefab.UnitPrussianFusilier });
        }
        else if (unitType == UnitFusilier.BritishFusilier)
        {
            _entityManager.AddComponent<UnitType_Prefab>(RegimentFusilier);
            _entityManager.SetComponentData(RegimentFusilier, new UnitType_Prefab { UnitTypePrefab = spawnerPrefab.UnitBritishFusilier });
        }
    }
}
/// <summary>
/// REGISTER all highlights and pereslect into regiments buffer
/// </summary>
public class RegimentBufferHighlights : SystemBase
{
    private EndInitializationEntityCommandBufferSystem End_init;
    protected override void OnCreate()
    {
        RequireForUpdate(GetEntityQuery(typeof(RegimentInitHighlightsTAG)));
        End_init = World.GetExistingSystem<EndInitializationEntityCommandBufferSystem>();
    }
    protected override void OnUpdate()
    {
        EntityCommandBuffer.ParallelWriter ecb = End_init.CreateCommandBuffer().AsParallelWriter();
        Entities
            .WithAll<RegimentInitHighlightsTAG>()
            .WithBurst()
            .ForEach((Entity regiment, int entityInQueryIndex, in DynamicBuffer<Child> unit) =>
            {
                DynamicBuffer<RegimentHighlightsBuffer> HighlightsBuffer = GetBuffer<RegimentHighlightsBuffer>(regiment);
                DynamicBuffer<RegimentPreSelectBuffer> PreselectBuffer = GetBuffer<RegimentPreSelectBuffer>(regiment);
                NativeArray<Child> RegimentUnits = unit.ToNativeArray(Allocator.Temp);
                for (int i = 0; i < RegimentUnits.Length; i++)
                {
                    DynamicBuffer<LinkedEntityGroup> unitLinkedGroup = GetBuffer<LinkedEntityGroup>(RegimentUnits[i].Value);
                    HighlightsBuffer.Add(new RegimentHighlightsBuffer { RegimentUnitsHighlights = unitLinkedGroup[1].Value });
                    PreselectBuffer.Add(new RegimentPreSelectBuffer { UnitPreselect = unitLinkedGroup[2].Value });
                }
                RegimentUnits.Dispose();
                ecb.AddComponent<RegimentInitPreselectTAG>(entityInQueryIndex, regiment);
                ecb.RemoveComponent<RegimentInitHighlightsTAG>(entityInQueryIndex, regiment);
            }).Schedule();
        End_init.AddJobHandleForProducer(Dependency);
    }
}
/// <summary>
/// DISABLE all Highlights(Preselect and Selection) on units
/// </summary>
public class RegimentBufferHighlightsInit : SystemBase
{
    private BeginSimulationEntityCommandBufferSystem Begin_Sim;
    protected override void OnCreate()
    {
        RequireForUpdate(GetEntityQuery(typeof(RegimentInitPreselectTAG)));
        Begin_Sim = World.GetExistingSystem<BeginSimulationEntityCommandBufferSystem>();
    }
    protected override void OnUpdate()
    {
        EntityCommandBuffer.ParallelWriter ecb = Begin_Sim.CreateCommandBuffer().AsParallelWriter();
        //////////////////////////////////
        /// DISABLE UNITS HIGHLIGHTS
        //////////////////////////////////
        Entities
            .WithAll<RegimentInitPreselectTAG>()
            .WithBurst()
            .ForEach((Entity regiment, int entityInQueryIndex, in DynamicBuffer<RegimentHighlightsBuffer> unitHighlights) =>
            {
                NativeArray<RegimentHighlightsBuffer> RegimentHighlights = unitHighlights.ToNativeArray(Allocator.Temp);
                for (int i = 0; i < RegimentHighlights.Length; i++)
                {
                    ecb.AddComponent<Disabled>(entityInQueryIndex, RegimentHighlights[i].RegimentUnitsHighlights);
                }
                RegimentHighlights.Dispose();
            }).ScheduleParallel();
        Begin_Sim.AddJobHandleForProducer(Dependency);
        //////////////////////////////////
        /// DISABLE UNITS PRESELECTION
        //////////////////////////////////
        Entities
            .WithAll<RegimentInitPreselectTAG>()
            .WithBurst()
            .ForEach((Entity regiment, int entityInQueryIndex, in DynamicBuffer<RegimentPreSelectBuffer> unitHighlights) =>
            {
                NativeArray<RegimentPreSelectBuffer> RegimentPreselects = unitHighlights.ToNativeArray(Allocator.Temp);
                for (int i = 0; i < RegimentPreselects.Length; i++)
                {
                    ecb.AddComponent<Disabled>(entityInQueryIndex, RegimentPreselects[i].UnitPreselect);
                }
                RegimentPreselects.Dispose();
                ecb.RemoveComponent<RegimentInitPreselectTAG>(entityInQueryIndex, regiment);
            }).ScheduleParallel();
        Begin_Sim.AddJobHandleForProducer(Dependency);
    }
}
