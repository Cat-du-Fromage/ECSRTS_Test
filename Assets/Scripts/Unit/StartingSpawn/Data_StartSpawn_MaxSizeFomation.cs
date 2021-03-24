using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct Data_Fusilier_MaxSizeFomation : IComponentData
{
    public int sizeFusilier;
}

public struct Data_Cavalry_MaxSizeFomation : IComponentData
{
    public int sizeCavalry;
}
