using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class SelectionPlacementSystem : SystemBase
{
    int _dragPlacement;
    float3 _startPlacement;
    float3 _endPlacement;

    int _minFormationSize;
    int _maxFormationSize;

    int _offsetPlacementSize;

    protected override void OnCreate()
    {
        RequireForUpdate(GetEntityQuery(typeof(State_Selected)));
    }

    protected override void OnStartRunning()
    {
        _dragPlacement = 0;
        _minFormationSize = 5;
        _maxFormationSize = 0;
    }

    protected override void OnUpdate()
    {
        if(Input.GetMouseButtonDown(1))
        {
            _startPlacement = Input.mousePosition;
        }
        
        if(Input.GetMouseButton(1))
        {
            _endPlacement = Input.mousePosition;
            _dragPlacement = math.length(_endPlacement - _startPlacement) >= _minFormationSize ? 1 : 0;
        }

        if (Input.GetMouseButtonUp(1))
        {

        }

        Entities.ForEach((ref Translation translation, in Rotation rotation) => {
            // Implement the work to perform for each entity here.
            // You should only access data that is local or that is a
            // field on this job. Note that the 'rotation' parameter is
            // marked as 'in', which means it cannot be modified,
            // but allows this job to run in parallel with other jobs
            // that want to read Rotation component data.
            // For example,
            //     translation.Value += math.mul(rotation.Value, new float3(0, 0, 1)) * deltaTime;
        }).Schedule();
    }
}
