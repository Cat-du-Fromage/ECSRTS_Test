using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct Data_Placement_StartPosition : IComponentData
{
    public float3 value;
}
