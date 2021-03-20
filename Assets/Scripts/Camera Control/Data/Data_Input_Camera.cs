using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct Data_Input_Camera : IComponentData
{
    public float3 MoveAxis;
    public float ZoomAxis;
    public KeyCode leftShift;
    public KeyCode mouseMiddle;
}
