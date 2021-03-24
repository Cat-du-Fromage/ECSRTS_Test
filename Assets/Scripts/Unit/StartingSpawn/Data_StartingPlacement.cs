using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct Data_StartingPlacement : IComponentData
{
    public float3 StartInitPlacement;
}
