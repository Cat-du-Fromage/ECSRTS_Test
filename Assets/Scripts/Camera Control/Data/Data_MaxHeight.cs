using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct Data_MaxHeight : IComponentData
{
    public float value;
}
