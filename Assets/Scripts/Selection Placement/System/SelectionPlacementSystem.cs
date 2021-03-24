using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
[BurstCompile]
/// <summary>
/// Input Manager for placement
/// </summary>
public class SelectionPlacementSystem : SystemBase
{
    EntityManager _entityManager;
    protected override void OnCreate()
    {
        RequireForUpdate(GetEntityQuery(typeof(State_Selected)));
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    protected override void OnUpdate()
    {
        Entities
        .WithAll<Tag_UnitPlacement>()
        .WithNone<Trigger_Placement_Regiment>()
        .WithBurst()
        .ForEach((Entity placHolder, ref Data_Placement_StartPosition startPosition, ref Data_Placement_EndPosition endPosition, ref Data_Placement_Length length, ref Data_Placement_LastLength lastLength) => 
        {
            if(Input.GetMouseButtonDown(1))
            {
                startPosition.value = Input.mousePosition;
            }
            
            if(Input.GetMouseButton(1))
            {
                endPosition.value = Input.mousePosition;
                length.value = math.length(endPosition.value - startPosition.value);
            }
            /*
            if(length.value != lastLength.value)
            {
                _entityManager.AddComponent<Trigger_Placement_Regiment>(placHolder);
                test.Enabled = true;
                lastLength.value = length.value;
            }
            */

        }).Run();

        var PlaHolder = GetSingletonEntity<Tag_UnitPlacement>();
        var length = _entityManager.GetComponentData<Data_Placement_Length>(PlaHolder).value;
        var lastLength = _entityManager.GetComponentData<Data_Placement_LastLength>(PlaHolder).value;

        if(length != lastLength)
        {
            _entityManager.AddComponent<Trigger_Placement_Regiment>(PlaHolder);
            _entityManager.SetComponentData(PlaHolder, new Data_Placement_LastLength{value = length});
        }
    }
}
