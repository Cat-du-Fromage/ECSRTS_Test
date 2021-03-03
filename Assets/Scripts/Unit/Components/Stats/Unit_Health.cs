using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct Unit_Health : IComponentData
{
    public int Health;
}
