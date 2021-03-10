using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class SelectionSystemV2 : SystemBase
{
    private float3 _startPosition;
    private float3 _endPosition;

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

        #region Left Click UP
        if (Input.GetMouseButtonUp(0))
        {
            if(!Input.GetKey(KeyCode.LeftShift))
            {
                //deselectAll
            }

            //Check wich Unit is within the rectangle or around the mouse(see MonkeyCode video RTS)

        }
        #endregion Left Click UP
    }
}
