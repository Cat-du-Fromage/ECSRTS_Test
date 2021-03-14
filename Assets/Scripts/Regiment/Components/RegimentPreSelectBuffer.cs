using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct RegimentPreSelectBuffer : IBufferElementData
{
    public Entity UnitPreselect;
}
