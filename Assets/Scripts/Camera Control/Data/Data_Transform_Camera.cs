using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct Data_Transform_Camera : IComponentData
{
    public float3 direction;
    public float speed;
    public float rotationSpeed;
    public float zoomSpeed;
    public float3 startPos;
    public float3 endPos;
}
