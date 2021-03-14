using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Physics;
using Unity.Physics.Systems;
//=================================================================================================================================
/// <summary>
/// Always Fire by getting the singelton <HoverEventHolderTag>
/// </summary>
//=================================================================================================================================
public class HoverSystem : SystemBase
{
    //HoverSystem
    private EntityManager _entityManager;
    private float3 _OldstartMousePos;
    private float3 _startMousePos;
    //HoverBox
    private float3 _startPositionFIX;
    private float3 _endPositionFIX;

    private Entity _eventHolderSelect;
    private Entity _unitHit;
    //Entity _previousEntityHit;
    private Entity _previousRegimentHit;
    private Entity _regimentUnitHit;
    UnityEngine.Ray ray;

    ////////////////////////
    //Selection Box
    ////////////////////////
    private int _dragSelection;
    private float _widthBoxSelect;
    private float _heightBoxSelect;

    private float _selectionBoxMinSize;
    private float3 _lowerLeftPosition;
    private float3 _upperRightPosition;

    BeginInitializationEntityCommandBufferSystem ECB_bInit;

    protected override void OnCreate()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        //Initialize fields
        _OldstartMousePos = Input.mousePosition;
        _startMousePos = Input.mousePosition;
        _unitHit = Entity.Null;
        _regimentUnitHit = Entity.Null;
        _previousRegimentHit = Entity.Null;
        _selectionBoxMinSize = 10f; // careful of the radius or we ended selecting 2 unit at a time
        ECB_bInit = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    protected override void OnStartRunning()
    {
        _dragSelection = 0;
        _eventHolderSelect = GetSingletonEntity<HoverEventHolderTag>();
        ray = Camera.main.ScreenPointToRay(_startMousePos);
    }

    protected override void OnUpdate()
    {
        // On hover Unit add Hover on unit
        if(_dragSelection == 0) //
        {
            //check when mouse change position
            if (!_startMousePos.Equals(Input.mousePosition))
            {
                _startMousePos = Input.mousePosition;
                ray = Camera.main.ScreenPointToRay(_startMousePos);
                _unitHit = RaycastUtils.Raycast(ray.origin, ray.direction * 50000f, (uint)RaycastUtils.CollisionLayer.UnitCollision);
                _regimentUnitHit = _unitHit != Entity.Null ? GetComponent<Parent>(_unitHit).Value : Entity.Null;
            }
            // On hover Check if we enter or exit
            if (_regimentUnitHit != _previousRegimentHit)
            {
                if (_regimentUnitHit == Entity.Null) //we enter the unit
                {
                    _entityManager.AddComponent<ExitHoverTag>(_previousRegimentHit); //fire the system for disabling preselections
                }
                else if (_previousRegimentHit == Entity.Null)
                {
                    _entityManager.AddComponent<HoverTag>(_regimentUnitHit); //Add Hovertag to the regiment
                    _entityManager.AddComponent<EnterHoverTag>(_regimentUnitHit); //Add preselection to current preselection
                }
                else // we exit the unit
                {
                    _entityManager.AddComponent<ExitHoverTag>(_previousRegimentHit); //Remove preselect from previous regiment
                    _entityManager.AddComponent<HoverTag>(_regimentUnitHit); //Add Hovertag to the regiment
                    _entityManager.AddComponent<EnterHoverTag>(_regimentUnitHit); //Add preselection to current preselection
                }
                _previousRegimentHit = _regimentUnitHit;
            }
        }

        #region Left Click Down
        //On Hover with a selection box
        if (Input.GetMouseButtonDown(0))
        {
            SelectionCanvasMono.instance.selectionBox.gameObject.SetActive(true); //SelectionBox SHOW
            _startPositionFIX = Input.mousePosition;
        }
        #endregion Left Click Down

        #region Left Click MAINTAIN
        if (Input.GetMouseButton(0))
        {
            _endPositionFIX = Input.mousePosition;
            #region BOX SELECTION
            _dragSelection = math.length(_startPositionFIX - (float3)Input.mousePosition) > 10 ? 1 : 0; // box selection?

            _widthBoxSelect = Input.mousePosition.x - _startPositionFIX.x;
            _heightBoxSelect = Input.mousePosition.y - _startPositionFIX.y;

            SelectionCanvasMono.instance.selectionBox.sizeDelta = new float2(math.abs(_widthBoxSelect), math.abs(_heightBoxSelect));
            SelectionCanvasMono.instance.selectionBox.anchoredPosition = new float2(_startPositionFIX.x, _startPositionFIX.y) + new float2(_widthBoxSelect / 2, _heightBoxSelect / 2);
            #endregion BOX SELECTION

            _lowerLeftPosition = new float3(math.min(_startPositionFIX.x, _endPositionFIX.x), math.min(_startPositionFIX.y, _endPositionFIX.y), 0);
            _upperRightPosition = new float3(math.max(_startPositionFIX.x, _endPositionFIX.x), math.max(_startPositionFIX.y, _endPositionFIX.y), 0);

            EntityCommandBuffer.ParallelWriter ecb = ECB_bInit.CreateCommandBuffer().AsParallelWriter();
            /////////////////////////
            ///ISSUE Need to convert Camera in order to use ECB
            ////////////////////////
            
            /*
            Entities
                .WithAll<UnitTag>()//Select Only Entities wit at least this component
                .WithBurst()
                .ForEach((Entity unit, int entityInQueryIndex, in Translation translation, in Parent regiment) =>
                {
                    //add 
                    float3 unitPosition = translation.Value;
                    float3 screenPos = Camera.main.WorldToScreenPoint(unitPosition);
                    if ((screenPos.x >= _lowerLeftPosition.x) && (screenPos.y >= _lowerLeftPosition.y) && (screenPos.x <= _upperRightPosition.x) && (screenPos.y <= _upperRightPosition.y))
                    {
                        ecb.AddComponent<SelectedUnitTag>(entityInQueryIndex, unit); // Add SelectionComponent : ATTENTION: NEED ".WithStructuralChanges()" to work
                        ecb.AddComponent<UnitNeedHighlightTag>(entityInQueryIndex, unit);
                        //Debug.Log(unit);
                    }
                }).Schedule();
            ECB_bInit.AddJobHandleForProducer(Dependency);
            */
        }
        #endregion Left Click MAINTAIN

        #region Left Click UP
        if (Input.GetMouseButtonUp(0))
        {
            _dragSelection = 0;
            SelectionCanvasMono.instance.selectionBox.gameObject.SetActive(false); //SelectionBox HIDE
        }
        #endregion Left Click UP
    }
}
