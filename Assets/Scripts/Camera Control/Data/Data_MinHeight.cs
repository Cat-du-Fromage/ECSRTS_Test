using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct Data_MinHeight : IComponentData
{
    public float value;
}
