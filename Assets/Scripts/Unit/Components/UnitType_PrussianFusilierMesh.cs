using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Rendering;

[GenerateAuthoringComponent]
public class UnitType_PrussianFusilierMeshChanger : IComponentData
{
    public UnityEngine.Mesh MeshPrussFusilier;
    public UnityEngine.Material MaterialPrussFusilier;
    public Entity prefab;
}
