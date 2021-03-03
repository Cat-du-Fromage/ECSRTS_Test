using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Rendering;

public class RegimentsSystem : SystemBase
{
    private EntityManager _entityManager;
    private EntityArchetype _regimentArchetype;

    protected override void OnCreate()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        _regimentArchetype = _entityManager.CreateArchetype(typeof(RegimentTag),typeof(LocalToWorld),typeof(Translation)/*,typeof(RenderBounds)*/); // set the archetype of a new Regiment
    }

    protected override void OnStartRunning()
    {
        EntityQuery spawnerQ = GetEntityQuery(typeof(SpawnerUnitsTag));
        Entity spawner = spawnerQ.GetSingletonEntity();
        Spawn_PrussianFusilier spawnerPrefab = _entityManager.GetComponentData<Spawn_PrussianFusilier>(spawner);
    }

    protected override void OnUpdate()
    {
        //Simulation of the spawning of regiment(like in total war)

        //Improvement to make:
        //1)register Units for each click
        //2) when press "enter" spawn all 
        if(Input.GetKeyDown(KeyCode.L))
        {
            EntityQuery spawnerQ = GetEntityQuery(typeof(SpawnerUnitsTag));
            Entity spawner = spawnerQ.GetSingletonEntity();
            Spawn_PrussianFusilier spawnerPrefab = _entityManager.GetComponentData<Spawn_PrussianFusilier>(spawner);

            Entity RegFusiPrus = _entityManager.CreateEntity(_regimentArchetype);
            _entityManager.AddComponent<CompRegimentClass_Fusilier>(RegFusiPrus);
            _entityManager.SetComponentData(RegFusiPrus, new CompRegimentClass_Fusilier { Size = 10 });//find a way to assign a number to a class Component
            _entityManager.AddComponent<RegimentUnassignedTag>(RegFusiPrus);
            _entityManager.AddComponent<CompUnitType_PrussianFusilier>(RegFusiPrus);
            //add Prefab for Units
            _entityManager.AddComponent<Spawn_PrussianFusilier>(RegFusiPrus);
            _entityManager.SetComponentData<Spawn_PrussianFusilier>(RegFusiPrus, new Spawn_PrussianFusilier { PrusFusilier = spawnerPrefab.PrusFusilier, BritishFusilier = spawnerPrefab.BritishFusilier });

            //Debug.Log("la taille de la troupe est : "+_entityManager.GetComponentData<CompRegimentClass_Fusilier>(RegFusiPrus).Size);
        }
    }
}
