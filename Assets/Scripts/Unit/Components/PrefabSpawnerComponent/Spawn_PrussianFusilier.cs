using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct Spawn_PrussianFusilier : IComponentData
{
    public Entity PrusFusilier;
    public Entity BritishFusilier;
}
