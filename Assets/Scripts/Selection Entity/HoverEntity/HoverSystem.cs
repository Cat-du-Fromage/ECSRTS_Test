using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Physics;
using Unity.Physics.Systems;
/*
public class HoverSystem : SystemBase
{
    float3 _OldstartMousePos;
    float3 _startMousePos;

    protected override void OnCreate()
    {
        base.OnCreate();
        _OldstartMousePos = Input.mousePosition;
        _startMousePos = Input.mousePosition;
    }

    #region RAYCAST ECS

    //==========================================================================================================================
    /// <summary>
    /// ECS RAYCAST BASIC Construction
    /// </summary>
    /// <param name="fromPosition"></param>
    /// <param name="toPosition"></param>
    /// <returns></returns>
    private Entity Raycast(float3 fromPosition, float3 toPosition)
    {
        BuildPhysicsWorld buildPhysicsWorld = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>(); //Seems to connect physics to the current world
        CollisionWorld collisionWorld = buildPhysicsWorld.PhysicsWorld.CollisionWorld;//Seems to connect methods uses for collision to the physics world we created

        RaycastInput raycastInput = new RaycastInput
        {
            Start = fromPosition,
            End = toPosition,
            //Layer filter
            Filter = new CollisionFilter
            {
                BelongsTo = ~0u, //belongs to all layers
                CollidesWith = ~0u, //collides with all layers
                GroupIndex = 0,
            }
        };
        //throw a raycast
        Unity.Physics.RaycastHit raycastHit = new Unity.Physics.RaycastHit();
        if (collisionWorld.CastRay(raycastInput, out raycastHit))
        {
            //Return the entity hit
            Entity hitEntity = buildPhysicsWorld.PhysicsWorld.Bodies[raycastHit.RigidBodyIndex].Entity;
            return hitEntity;
        }
        else
        {
            return Entity.Null;
        }

    }

    //==========================================================================================================================
    #endregion RAYCAST ECS

    protected override void OnUpdate()
    {
        // On hover Unit add Hover on unit
        if (!_startMousePos.Equals(Input.mousePosition))
        {
            Debug.Log("mouse change");
            _startMousePos = Input.mousePosition;
        }

        UnityEngine.Ray ray = Camera.main.ScreenPointToRay(_startMousePos);
        Entity _UnitHit = Raycast(ray.origin, ray.direction * 50000f);

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
*/