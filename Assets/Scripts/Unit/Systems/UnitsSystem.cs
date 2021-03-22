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
    EntityManager _entityManager;
    EntityQuery _unassignedRegiment;
    Entity _singeltonInitStartPos;
    float3 _startInitPos;
    BeginInitializationEntityCommandBufferSystem BeginInit_ECB;

    protected override void OnCreate()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        this._unassignedRegiment = GetEntityQuery(typeof(State_Unassigned));
        BeginInit_ECB = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    protected override void OnStartRunning()
    {
        _singeltonInitStartPos = GetSingletonEntity<Data_StartingPlacement>();
        _startInitPos = _entityManager.GetComponentData<Data_StartingPlacement>(_singeltonInitStartPos).StartInitPlacement;
    }

    protected override void OnUpdate()
    {

        if(Input.GetKeyDown(KeyCode.P)) //PlaceHolder event
        {
            EntityCommandBuffer.ParallelWriter BeginInitecb = BeginInit_ECB.CreateCommandBuffer().AsParallelWriter(); // done at the begining
            Entities
                .WithName("UNITSPAWN")
                .WithBurst()
                .WithAll<State_Unassigned, CompRegimentClass_Fusilier>()
                .ForEach((Entity Regiment, int entityInQueryIndex, in Data_StartingPlacement data_StartingPlacement ,in CompRegimentClass_Fusilier RegimentSize, in UnitType_Prefab prefab) =>
                {
                    //allocate memory
                    //NativeArray<Entity> RegimentUnits = new NativeArray<Entity>(RegimentSize.Size, Allocator.Temp);
                    for (int i = 0; i < RegimentSize.Size; i++)
                    {
                        Entity Unit = BeginInitecb.Instantiate(entityInQueryIndex, prefab.UnitTypePrefab);
                        BeginInitecb.AddComponent<UnitTag>(entityInQueryIndex, Unit);
                        BeginInitecb.SetComponent(entityInQueryIndex, Unit, new Translation { Value = new float3(data_StartingPlacement .StartInitPlacement.x+ i, data_StartingPlacement.StartInitPlacement.y+2, data_StartingPlacement.StartInitPlacement.z) });
                        BeginInitecb.AddComponent(entityInQueryIndex, Unit, new Parent { Value = Regiment });
                        BeginInitecb.AddComponent(entityInQueryIndex, Unit, new LocalToParent());
                    } //GetBuffer<Child>(Unit)[0]
                    //RegimentUnits.Dispose();
                    BeginInitecb.AddComponent<RegimentInitHighlightsTAG>(entityInQueryIndex, Regiment);
                    BeginInitecb.RemoveComponent<State_Unassigned>(entityInQueryIndex, Regiment);
                }).ScheduleParallel(); // Execute in parallel for each chunk of entities
            BeginInit_ECB.AddJobHandleForProducer(this.Dependency);
        }
    }
}
