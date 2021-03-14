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
public class UnitsSystem : SystemBase
{
    private EntityManager _entityManager;
    private EntityQuery _unassignedRegiment;
    BeginInitializationEntityCommandBufferSystem BeginInit_ECB;

    protected override void OnCreate()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        this._unassignedRegiment = GetEntityQuery(typeof(RegimentUnassignedTag));
        BeginInit_ECB = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {

        if(Input.GetKeyDown(KeyCode.P)) //PlaceHolder event
        {
            EntityCommandBuffer.ParallelWriter BeginInitecb = BeginInit_ECB.CreateCommandBuffer().AsParallelWriter(); // done at the begining
            Entities
                .WithName("UNITSPAWN")
                .WithBurst()
                .WithAll<RegimentUnassignedTag, CompRegimentClass_Fusilier>()
                .ForEach((Entity Regiment, int entityInQueryIndex, in CompRegimentClass_Fusilier RegimentSize, in UnitType_Prefab prefab) =>
                {
                    //allocate memory
                    NativeArray<Entity> RegimentUnits = new NativeArray<Entity>(RegimentSize.Size, Allocator.Temp);
                    for (int i = 0; i < RegimentUnits.Length; i++)
                    {
                        Entity Unit = BeginInitecb.Instantiate(entityInQueryIndex, prefab.UnitTypePrefab);
                        BeginInitecb.AddComponent<UnitTag>(entityInQueryIndex, Unit);
                        BeginInitecb.SetComponent(entityInQueryIndex, Unit, new Translation { Value = new float3(8+i, 5, 5) });
                        BeginInitecb.AddComponent(entityInQueryIndex, Unit, new Parent { Value = Regiment });
                        BeginInitecb.AddComponent(entityInQueryIndex, Unit, new LocalToParent());
                    } //GetBuffer<Child>(Unit)[0]
                    RegimentUnits.Dispose();
                    BeginInitecb.AddComponent<RegimentInitHighlightsTAG>(entityInQueryIndex, Regiment);
                    BeginInitecb.RemoveComponent<RegimentUnassignedTag>(entityInQueryIndex, Regiment);
                }).ScheduleParallel(); // Execute in parallel for each chunk of entities
            BeginInit_ECB.AddJobHandleForProducer(this.Dependency);
        }
    }
}
