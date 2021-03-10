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

    protected override void OnCreate()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        BeginInit_ECB = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {

        if(Input.GetKeyDown(KeyCode.P))
        {
            //PROBLEME
            //il semblerait le le regiment dans le lambda ne soit qu'une copie temporaire
            //solution 2
            //stocker l'id de la copie dans les unit�s puis les r�assigner dans une autre boucle
            // RESOLVE! ne regiment lack : LocalToWorld
            /*
            EntityArchetype archetypeUnits = _entityManager.CreateArchetype(
            typeof(Translation),
            typeof(Rotation),
            typeof(RenderMesh),
            typeof(RenderBounds),
            typeof(LocalToWorld)
            );
            */
            EntityCommandBuffer.ParallelWriter BeginInitecb = BeginInit_ECB.CreateCommandBuffer().AsParallelWriter(); // done at the begining
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

        }
    }
}
