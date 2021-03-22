using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct Cursor_Start_Position : IComponentData
{
    public float3 StartPosition;
}

[Serializable]
public struct Cursor_End_Position : IComponentData
{
    public float3 EndPosition;
}