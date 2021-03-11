using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Physics;
using Unity.Physics.Systems;


[AlwaysUpdateSystem]
public class SelectionSystemV2 : SystemBase
{
    private float3 _startPosition;
    private float3 _endPosition;

    ////////////////////////
    //Selection Box
    ////////////////////////
    private bool _dragSelection;
    private float _widthBoxSelect;
    private float _heightBoxSelect;

    private float _selectionBoxMinSize;
    private float3 _lowerLeftPosition;
    private float3 _upperRightPosition;

    private EntityManager _entityManager;


    #region RAYCAST ECS

    //==========================================================================================================================
    /// <summary>
    /// ECS RAYCAST BASIC Construction
    /// </summary>
    /// <param name="fromPosition"></param>
    /// <param name="toPosition"></param>
    /// <returns></returns>
    private Entity Raycast(float3 fromPosition, float3 toPosition, uint collisionFilter)
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
                CollidesWith = collisionFilter, //collides with all layers
                GroupIndex = 0,
            }
        };
        Debug.Log("raycastInput " + raycastInput.Filter.CollidesWith);
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

    protected override void OnCreate()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        base.OnCreate();
        _selectionBoxMinSize = 10f; // careful of the radius or we ended selecting 2 unit at a time
    }

    protected override void OnStartRunning()
    {
        base.OnStartRunning();

    }
    protected override void OnUpdate()
    {
        #region Left Click Down
        if (Input.GetMouseButtonDown(0))
        {
            SelectionCanvasMono.instance.selectionBox.gameObject.SetActive(true); //SelectionBox HIDE
            //GET the mouse Start Position
            _startPosition = Input.mousePosition;

            float3 BoxPositionNoZ = new float3(_startPosition.x, _startPosition.y, 0);
            _lowerLeftPosition = BoxPositionNoZ + new float3(-1, -1, 0) * _selectionBoxMinSize * 0.5f;
            Debug.Log(_lowerLeftPosition);
            _upperRightPosition = BoxPositionNoZ + new float3(1, 1, 0) * _selectionBoxMinSize * 0.5f;
            Debug.Log(_upperRightPosition);
            UnityEngine.Ray ray = Camera.main.ScreenPointToRay(_startPosition);

            var filter = new CollisionFilter
            {
                CollidesWith = 2u
            };
            Entity _UnitHit = Raycast(ray.origin, ray.direction * 50000f, 2u);
            if(_UnitHit != Entity.Null)
            {
                BuildPhysicsWorld buildPhysicsWorld = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>(); //Seems to connect physics to the current world
                Debug.Log("FILTER WORKS" + " " + filter.CollidesWith.ToString() + " " + _UnitHit);
            }
            else
            {
                Debug.Log("FILTER DONT WORKS" + " " + filter.CollidesWith + " " + _UnitHit);
            }

            /*
            Entities
            .WithName("OnClickSelected")
            .WithStructuralChanges() // allow to use MainThread structural change , CAREFULL this does not allow BURST COMPILE
            .WithAll<UnitTag>()//Select Only Entities wit at least this component
            .WithNone<SelectedUnitTag>()
            .ForEach((Entity entity, in Translation translation) =>
            {
                float3 entityPosition = translation.Value;
                float3 screenPos = Camera.main.WorldToScreenPoint(entityPosition);
                if ((screenPos.x >= _lowerLeftPosition.x) && (screenPos.y >= _lowerLeftPosition.y) && (screenPos.x <= _upperRightPosition.x) && (screenPos.y <= _upperRightPosition.y))
                {
                    _entityManager.AddComponent<SelectedUnitTag>(entity); // Add SelectionComponent : ATTENTION: NEED ".WithStructuralChanges()" to work
                    _entityManager.AddComponent<UnitNeedHighlightTag>(entity);
                    Debug.Log(entity);
                }
            })
            .WithoutBurst()
            .Run();
            */
        }
        #endregion Left Click Down

        #region Left Click MAINTAIN
        if (Input.GetMouseButton(0))
        {
            //Selection Rectangle
            _dragSelection = math.length(_startPosition - (float3)Input.mousePosition) > 10 ? true : false;

            _widthBoxSelect = Input.mousePosition.x - _startPosition.x;
            _heightBoxSelect = Input.mousePosition.y - _startPosition.y;

            SelectionCanvasMono.instance.selectionBox.sizeDelta = new float2(math.abs(_widthBoxSelect), math.abs(_heightBoxSelect));
            SelectionCanvasMono.instance.selectionBox.anchoredPosition = new float2(_startPosition.x, _startPosition.y) + new float2(_widthBoxSelect / 2, _heightBoxSelect / 2);
        }
        #endregion Left Click MAINTAIN

        #region Left Click UP
        if (Input.GetMouseButtonUp(0))
        {
            SelectionCanvasMono.instance.selectionBox.gameObject.SetActive(false); //SelectionBox HIDE

            if (!Input.GetKey(KeyCode.LeftShift))
            {
                //deselectAll
            }

            //Check wich Unit is within the rectangle or around the mouse(see MonkeyCode video RTS)

        }
        #endregion Left Click UP
    }

    private void template()
    {
        _endPosition = Input.mousePosition;
        _lowerLeftPosition = new float3(math.min(_startPosition.x, _endPosition.x), math.min(_startPosition.y, _endPosition.y), 0);
        _upperRightPosition = new float3(math.max(_startPosition.x, _endPosition.x), math.max(_startPosition.y, _endPosition.y), 0);
        Entities
            .WithStructuralChanges() // allow to use MainThread structural change , CAREFULL this does not allow BURST COMPILE
            .WithAll<UnitTag>()//Select Only Entities wit at least this component
            .ForEach((Entity entity, in Translation translation) =>
            {
                float3 entityPosition = translation.Value;
                float3 screenPos = Camera.main.WorldToScreenPoint(entityPosition);
                if ((screenPos.x >= _lowerLeftPosition.x) && (screenPos.y >= _lowerLeftPosition.y) && (screenPos.x <= _upperRightPosition.x) && (screenPos.y <= _upperRightPosition.y))
                {
                    _entityManager.AddComponent<SelectedUnitTag>(entity); // Add SelectionComponent : ATTENTION: NEED ".WithStructuralChanges()" to work
                            _entityManager.AddComponent<UnitNeedHighlightTag>(entity);
                    Debug.Log(entity);
                }
            })
            .WithoutBurst()
            .Run();
    }
}
