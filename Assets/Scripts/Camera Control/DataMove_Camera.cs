using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[GenerateAuthoringComponent]
public struct DataMove_Camera : IComponentData
{
    public float3 direction;
    public float _speed;
    public float _zoomSpeed;
    public float _rotationSpeed;
}
