using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Unity.Physics;

//only allow the update after regiments are created
[UpdateAfter(typeof(RegimentsSystem))]
public class UnitsSystem : SystemBase
{
    private EntityManager _entityManager;
    BeginInitializationEntityCommandBufferSystem BeginInit_ECB;
    EndInitializationEntityCommandBufferSystem EndInit_ECB;

    EndSimulationEntityCommandBufferSystem ecsSim;
    protected override void OnCreate()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        BeginInit_ECB = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        EndInit_ECB = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
        ecsSim = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnStartRunning()
    {
        /*
        EntityQuery spawnerQ = GetEntityQuery(typeof(SpawnerUnitsTag));
        Entity spawner = spawnerQ.GetSingletonEntity();
        Spawn_PrussianFusilier spawnerPrefab = _entityManager.GetComponentData<Spawn_PrussianFusilier>(spawner);
        */
    }

    protected override void OnUpdate()
    {

        if(Input.GetKeyDown(KeyCode.P))
        {
            //PROBLEME
            //il semblerait le le regiment dans le lambda ne soit qu'une copie temporaire
            //solution 2
            //stocker l'id de la copie dans les unités puis les réassigner dans une autre boucle
            // RESOLVE! ne regiment lack : LocalToWorld
            EntityArchetype archetypeUnits = _entityManager.CreateArchetype(
            typeof(Translation),
            typeof(Rotation),
            typeof(RenderMesh),
            typeof(RenderBounds),
            typeof(LocalToWorld)
            );
            EntityCommandBuffer.ParallelWriter BeginInitecb = BeginInit_ECB.CreateCommandBuffer().AsParallelWriter(); // done at the begining
            EntityCommandBuffer.ParallelWriter EndInitecb = EndInit_ECB.CreateCommandBuffer().AsParallelWriter(); //Done at the end
            Entities
                .WithBurst()
                .WithAll<RegimentUnassignedTag, CompRegimentClass_Fusilier>()
                .ForEach((Entity Regiment, int entityInQueryIndex, in CompRegimentClass_Fusilier RegimentSize, in UnitType_Prefab prefab) =>
                {
                    for (int i = 0; i < RegimentSize.Size; i++)
                    {
                        //Entity Unit = BeginInitecb.CreateEntity(entityInQueryIndex, archetypeUnits);
                        Entity Unit = BeginInitecb.Instantiate(entityInQueryIndex, prefab.UnitTypePrefab);
                        BeginInitecb.AddComponent<UnitTag>(entityInQueryIndex, Unit);
                        BeginInitecb.SetComponent(entityInQueryIndex, Unit, new Translation { Value = new float3(8+i, 5, 5) });
                        BeginInitecb.AddComponent(entityInQueryIndex, Unit, new Parent { Value = Regiment });
                        BeginInitecb.AddComponent(entityInQueryIndex, Unit, new LocalToParent());
                    }
                    BeginInitecb.RemoveComponent<RegimentUnassignedTag>(entityInQueryIndex, Regiment);
                }).ScheduleParallel(); // Execute in parallel for each chunk of entities
            BeginInit_ECB.AddJobHandleForProducer(Dependency);
            EndInit_ECB.AddJobHandleForProducer(Dependency);
        }
    }
}
