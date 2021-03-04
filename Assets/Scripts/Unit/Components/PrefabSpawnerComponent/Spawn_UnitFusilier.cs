using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct Spawn_UnitFusilier : IComponentData
{
    public Entity UnitPrussianFusilier;
    public Entity UnitBritishFusilier;
    public Entity UnitFrenchFusilier;
}
