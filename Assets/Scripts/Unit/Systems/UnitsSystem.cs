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
                .ForEach((Entity Regiment, int entityInQueryIndex, ref Data_StartingPlacement data_StartingPlacement, in LocalToWorld ltw, in CompRegimentClass_Fusilier RegimentSize, in UnitType_Prefab prefab, in Data_Unit_PositionOffset offetPos) =>
                {
                    //allocate memory
                    //check if there is > 1 Regiment
                    int maxSize = 10;
                    int row = 0;
                    data_StartingPlacement.StartInitPlacement.x += math.mul(math.mul(entityInQueryIndex, maxSize + 3), offetPos.PosOffset+ltw.Value.c0.x); //set the new spawning point depending on the number of regiment already spawned
                    //Debug.Log($"entityInQueryIndex ={entityInQueryIndex}"); // now we know entityInQueryIndex began at 0
                    for (int i = 0; i < RegimentSize.Size; i++)
                    {
                        Entity Unit = BeginInitecb.Instantiate(entityInQueryIndex, prefab.UnitTypePrefab);
                        BeginInitecb.AddComponent<UnitTag>(entityInQueryIndex, Unit);
                        BeginInitecb.AddComponent(entityInQueryIndex, Unit, new Parent { Value = Regiment });
                        BeginInitecb.AddComponent(entityInQueryIndex, Unit, new LocalToParent());
                        //check if the maxSize (for starting spawn) is reach, add +1 row if so
                        if(i % maxSize == 0 && i != 0)
                        {
                            row += 1;
                        }
                        // x = start.x + (i%10(only get unity 0-9)) * (offset + unitScale) // z = start.z - row * (offset + unitScale)
                        BeginInitecb.SetComponent(entityInQueryIndex, Unit, new Translation { Value = new float3(data_StartingPlacement.StartInitPlacement.x + math.mul((i % maxSize),(offetPos.PosOffset+ltw.Value.c0.x)), data_StartingPlacement.StartInitPlacement.y+2, data_StartingPlacement.StartInitPlacement.z - math.mul(row, offetPos.PosOffset+ltw.Value.c0.x)) });
                    } //GetBuffer<Child>(Unit)[0]
                    data_StartingPlacement.StartInitPlacement = new float3(math.right()); //set direction of regiment for futur placement
                    BeginInitecb.AddComponent<RegimentInitHighlightsTAG>(entityInQueryIndex, Regiment);
                    BeginInitecb.RemoveComponent<State_Unassigned>(entityInQueryIndex, Regiment);
                }).ScheduleParallel(); // Execute in parallel for each chunk of entities
            BeginInit_ECB.AddJobHandleForProducer(this.Dependency);
        }
    }
}
