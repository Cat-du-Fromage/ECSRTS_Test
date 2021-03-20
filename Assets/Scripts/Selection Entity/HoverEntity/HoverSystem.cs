using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Physics;
using Unity.Physics.Systems;
using CameraUtils;
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
                    _entityManager.AddComponent<Trigger_Disable_Hover>(_previousRegimentHit); //fire the system for disabling preselections
                }
                else if (_previousRegimentHit == Entity.Null)
                {
                    _entityManager.AddComponent<State_Hovered>(_regimentUnitHit); //Add Hovertag to the regiment
                    _entityManager.AddComponent<Trigger_Enable_Hover>(_regimentUnitHit); //Add preselection to current preselection
                }
                else // we exit the unit
                {
                    _entityManager.AddComponent<Trigger_Disable_Hover>(_previousRegimentHit); //Remove preselect from previous regiment
                    _entityManager.AddComponent<State_Hovered>(_regimentUnitHit); //Add Hovertag to the regiment
                    _entityManager.AddComponent<Trigger_Enable_Hover>(_regimentUnitHit); //Add preselection to current preselection
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
            EntityQuery Regimentselected = GetEntityQuery(typeof(State_Selected));
            EntityQuery RegimentHovered = GetEntityQuery(ComponentType.ReadOnly<State_Hovered>());
            //XXXXXXXXXXXXXXXXXXXXXXXXXXXX
            //SELECTION
            //XXXXXXXXXXXXXXXXXXXXXXXXXXXX
            #region Selection OnClickDown
            if(Input.GetKey(KeyCode.LeftControl))
            {
                if(_unitHit == Entity.Null)
                {
                    return;
                }
                else
                {
                    if (HasComponent<State_Selected>(_regimentUnitHit))
                    {
                        _entityManager.AddComponent<Trigger_Disable_Selection>(_regimentUnitHit); //trigger highlight disable
                    }
                    else
                    {
                        _entityManager.AddComponent<Trigger_Enable_Selection>(_regimentUnitHit); //trigger highlight enable
                    }
                }
            }
            else
            {
                //Query all unit selected except the one Hovering(so we dont deselect it)
                EntityQueryDesc RegimentSelectNoHover = new EntityQueryDesc
                {
                    All = new ComponentType[] { typeof(State_Selected) },
                    None = new ComponentType[] { typeof(State_Hovered) }
                };
                if (_unitHit == Entity.Null)
                {
                    _entityManager.AddComponent<Trigger_Disable_Selection>(Regimentselected); //trigger highlight disable
                }
                else
                {
                    _entityManager.AddComponent<Trigger_Disable_Selection>(GetEntityQuery(RegimentSelectNoHover)); //trigger highlight disable
                    if (!HasComponent<State_Selected>(_regimentUnitHit))
                    {
                        _entityManager.AddComponent<Trigger_Enable_Selection>(_regimentUnitHit);//trigger highlight enable
                    }
                }
            }

            #endregion Selection OnClickDown
        }
        #endregion Left Click Down

        #region Left Click MAINTAIN
        if (Input.GetMouseButton(0))
        {
            _endPositionFIX = Input.mousePosition;
            #region BOX SELECTION
            _dragSelection = math.length(_startPositionFIX - (float3)Input.mousePosition) > _selectionBoxMinSize ? 1 : 0; // box selection?

            _widthBoxSelect = Input.mousePosition.x - _startPositionFIX.x;
            _heightBoxSelect = Input.mousePosition.y - _startPositionFIX.y;

            SelectionCanvasMono.instance.selectionBox.sizeDelta = new float2(math.abs(_widthBoxSelect), math.abs(_heightBoxSelect));
            SelectionCanvasMono.instance.selectionBox.anchoredPosition = new float2(_startPositionFIX.x, _startPositionFIX.y) + new float2(_widthBoxSelect / 2, _heightBoxSelect / 2);
            #endregion BOX SELECTION
            //
            //Check only when Box selection actually exist
            
            if(_dragSelection == 1)
            {
                _lowerLeftPosition = new float3(math.min(_startPositionFIX.x, _endPositionFIX.x), math.min(_startPositionFIX.y, _endPositionFIX.y), 0);
                _upperRightPosition = new float3(math.max(_startPositionFIX.x, _endPositionFIX.x), math.max(_startPositionFIX.y, _endPositionFIX.y), 0);
                //=========================================================================
                //Add Pre Selection To the unit's regiment if inside the selection Box
                //Trigger : HoverHighlightSystem.cs - HoverHighligntEnable
                //=========================================================================
                Entities
                    .WithAll<RegimentTag>()
                    .WithNone<State_Hovered>()
                    .WithStructuralChanges()
                    .WithoutBurst()
                    .ForEach((Entity regiment, in DynamicBuffer<Child> units) =>
                    {
                        //NativeArray<Entity> unitsRegiment = units.Reinterpret<Entity>().ToNativeArray(Allocator.Temp);
                        for(int i = 0; i< units.Length; i++)
                        {
                            float3 unitPosition = GetComponent<Translation>(units[i].Value).Value;
                            float3 screenPos = Camera.main.WorldToScreenPoint(unitPosition);
                            if ((screenPos.x >= _lowerLeftPosition.x) && (screenPos.y >= _lowerLeftPosition.y) && (screenPos.x <= _upperRightPosition.x) && (screenPos.y <= _upperRightPosition.y))
                            {
                                _entityManager.AddComponent<State_Hovered>(regiment);
                                _entityManager.AddComponent<Trigger_Enable_Hover>(regiment);
                                break;
                            }
                        }
                        //unitsRegiment.Dispose();
                    }).Run();
                //=========================================================================
                //Remove Pre Selection from Regiment who have no units in the box selection
                //Trigger : HoverHighlightSystem.cs - HoverHighligntDisable
                //=========================================================================
                Entities
                    .WithAll<State_Hovered, RegimentTag>()
                    .WithoutBurst()
                    .WithStructuralChanges()
                    .ForEach((Entity Regiment, in DynamicBuffer<Child> unit) =>
                    {
                        NativeArray<Entity> AllUnits = unit.Reinterpret<Entity>().ToNativeArray(Allocator.Temp);
                        for (int i = 0; i < AllUnits.Length; i++)
                        {
                            float3 unitPosition = GetComponent<Translation>(AllUnits[i]).Value;
                            float3 screenPos = Camera.main.WorldToScreenPoint(unitPosition);
                            if ((screenPos.x >= _lowerLeftPosition.x) && (screenPos.y >= _lowerLeftPosition.y) && (screenPos.x <= _upperRightPosition.x) && (screenPos.y <= _upperRightPosition.y))
                            {
                                break;
                            }
                            if(i == AllUnits.Length - 1)
                            {
                                _entityManager.AddComponent<Trigger_Disable_Hover>(Regiment);
                            }
                        }
                        AllUnits.Dispose();
                    }).Run();
            }
            
        }
        #endregion Left Click MAINTAIN

        #region Left Click UP
        if (Input.GetMouseButtonUp(0))
        {
            SelectionCanvasMono.instance.selectionBox.gameObject.SetActive(false); //SelectionBox HIDE

            EntityQueryDesc HoveredNotSelect = new EntityQueryDesc //hovered but not selected
            {
                All = new ComponentType[] { typeof(State_Hovered) },
                None = new ComponentType[] { typeof(State_Selected) }
            };
            EntityQueryDesc SelectNoHover = new EntityQueryDesc //selected but not hovered
            {
                All = new ComponentType[] { typeof(State_Selected) },
                None = new ComponentType[] { typeof(State_Hovered) }
            };
            EntityQuery RegimentHovered = GetEntityQuery(ComponentType.ReadOnly<State_Hovered>());
            if (_dragSelection == 1)
            {
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    _entityManager.AddComponent<Trigger_Enable_Selection>(GetEntityQuery(HoveredNotSelect));
                }
                else
                {
                    _entityManager.AddComponent<Trigger_Disable_Selection>(GetEntityQuery(SelectNoHover));
                    _entityManager.AddComponent<Trigger_Enable_Selection>(GetEntityQuery(HoveredNotSelect));
                }


                _entityManager.AddComponent<Trigger_Disable_Hover>(RegimentHovered);
            }
            _dragSelection = 0;
        }
        #endregion Left Click UP
    }
}
