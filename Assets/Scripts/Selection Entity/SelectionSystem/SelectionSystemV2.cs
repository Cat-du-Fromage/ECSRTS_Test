using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
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

    private EntityManager _entityManager;
    protected override void OnCreate()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        base.OnCreate();
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
        float3 lowerLeftPosition = new float3(math.min(_startPosition.x, _endPosition.x), math.min(_startPosition.y, _endPosition.y), 0);
        float3 upperRightPosition = new float3(math.max(_startPosition.x, _endPosition.x), math.max(_startPosition.y, _endPosition.y), 0);
        Entities
            .WithStructuralChanges() // allow to use MainThread structural change , CAREFULL this does not allow BURST COMPILE
            .WithAll<UnitTag>()//Select Only Entities wit at least this component
            .ForEach((Entity entity, in Translation translation) =>
            {
                float3 entityPosition = translation.Value;
                float3 screenPos = Camera.main.WorldToScreenPoint(entityPosition);
                if ((screenPos.x >= lowerLeftPosition.x) && (screenPos.y >= lowerLeftPosition.y) && (screenPos.x <= upperRightPosition.x) && (screenPos.y <= upperRightPosition.y))
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

