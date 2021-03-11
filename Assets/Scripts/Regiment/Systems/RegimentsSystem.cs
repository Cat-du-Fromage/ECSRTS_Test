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

    private EntityManager _entityManager;
    private EntityArchetype _regimentArchetype;
    protected override void OnCreate()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        _regimentArchetype = _entityManager.CreateArchetype(typeof(RegimentTag),typeof(LocalToWorld),typeof(Translation),typeof(RenderBounds)); // set the archetype of a new Regiment
    }

    protected override void OnUpdate()
    {
        //Simulation of the spawning of regiment(like in total war)

        //Improvement to make:
        //1)register Units for each click
        //2) when press "enter" spawn all 
        if (Input.GetKeyDown(KeyCode.L))
        {
            CreateRegiment(UnitFusilier.PrussianFusilier);
        }
        else if (Input.GetKeyDown(KeyCode.K))
        {
            CreateRegiment(UnitFusilier.BritishFusilier);
        }
    }

    public void CreateRegiment(UnitFusilier unitType)
    {
        EntityQuery spawnerQ = GetEntityQuery(typeof(SpawnerUnitsTag));
        Entity spawner = spawnerQ.GetSingletonEntity();
        Spawn_UnitFusilier spawnerPrefab = _entityManager.GetComponentData<Spawn_UnitFusilier>(spawner);

        Entity RegimentFusilier = _entityManager.CreateEntity(_regimentArchetype);
        _entityManager.AddComponent<CompRegimentClass_Fusilier>(RegimentFusilier);
        _entityManager.SetComponentData(RegimentFusilier, new CompRegimentClass_Fusilier { Size = 10 });//find a way to assign a number to a class Component
        _entityManager.AddComponent<RegimentUnassignedTag>(RegimentFusilier);
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
