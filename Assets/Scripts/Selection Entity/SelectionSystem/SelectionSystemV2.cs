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

    protected override void OnUpdate()
    {
        #region Left Click Down
        if (Input.GetMouseButtonDown(0))
        {

        }
        #endregion Left Click Down

        #region Left Click
        if (Input.GetMouseButton(0))
        {

        }
        #endregion Left Click

        #region Left Click UP
        if (Input.GetMouseButtonUp(0))
        {

        }
        #endregion Left Click UP
    }
}
