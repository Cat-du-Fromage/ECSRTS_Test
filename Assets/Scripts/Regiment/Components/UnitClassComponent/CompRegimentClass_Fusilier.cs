using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct CompRegimentClass_Fusilier : IComponentData
{
    public int Size;
    /*
    public static implicit operator int(CompRegimentClass_Fusilier b) => b.Size;
    public static implicit operator CompRegimentClass_Fusilier(int v) => new CompRegimentClass_Fusilier { Size = v };
    public static CompRegimentClass_Fusilier Default => 10;
    //One Line version
    public static CompRegimentClass_Fusilier Default = new CompRegimentClass_Fusilier() { Size = 10 };
    */
}
