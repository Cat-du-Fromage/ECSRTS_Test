using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Physics;
using Unity.Physics.Systems;

public class HoverSystem : SystemBase
{
    EntityManager _entityManager;
    float3 _OldstartMousePos;
    float3 _startMousePos;

    Entity _eventHolderSelect;
    Entity _unitHit;
    Entity _previousEntityHit;
    Entity _regimentUnitHit;
    UnityEngine.Ray ray;

    protected override void OnCreate()
    {
        base.OnCreate();
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        //Initialize fields
        ray = Camera.main.ScreenPointToRay(_startMousePos);
        _OldstartMousePos = Input.mousePosition;
        _startMousePos = Input.mousePosition;
        _unitHit = Entity.Null;
        _regimentUnitHit = Entity.Null;
        _previousEntityHit = Entity.Null;
    }

    protected override void OnStartRunning()
    {
        base.OnStartRunning();
        _eventHolderSelect = GetSingletonEntity<HoverEventHolderTag>();
    }

    protected override void OnUpdate()
    {
        // On hover Unit add Hover on unit
        if (!_startMousePos.Equals(Input.mousePosition))
        {
            _startMousePos = Input.mousePosition;
            ray = Camera.main.ScreenPointToRay(_startMousePos);
            _unitHit = RaycastUtils.Raycast(ray.origin, ray.direction * 50000f, (uint)RaycastUtils.CollisionLayer.UnitCollision);
        }

        if(_unitHit != _previousEntityHit)
        {
            if(_unitHit != Entity.Null) //we enter the unit
            {
                //add hover tag on regiment(not unit)
                _regimentUnitHit = GetComponent<Parent>(_unitHit).Value;
                _entityManager.AddComponent<HoverTag>(_regimentUnitHit);
                _entityManager.AddComponent<EnterHoverTag>(_regimentUnitHit); //fire the system for enabling preselections
                Debug.Log("RegimenthoverAdded" + _regimentUnitHit);
                _previousEntityHit = _unitHit;
                //Debug.Log("RegimenthoverAdded _previousEntChange" + _previousEntityHit);
            }
            else // we exit the unit
            {
                Debug.Log("RegimenthoverRemove" + _regimentUnitHit);
                _entityManager.AddComponent<ExitHoverTag>(_regimentUnitHit); //fire the system for enabling preselections
                _previousEntityHit = _unitHit;
            }
        }

        #region Left Click Down
        if (Input.GetMouseButtonDown(0))
        {
            //GET the mouse Start Position

        }
        #endregion Left Click Down

        #region Left Click MAINTAIN
        if (Input.GetMouseButton(0))
        {
            //GET the mouse end position
            //draw rectangle

            //CHECK if distance between start and end position is > 5(DragSelection false/true?)
            //Set dragoSelection true or false
        }
        #endregion Left Click MAINTAIN
    }
}
