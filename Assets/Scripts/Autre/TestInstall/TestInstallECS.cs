/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;
/// <summary>
/// PreMade script to check if the package is installed correctly
/// </summary>
/// 
public class TestInstallECS : MonoBehaviour
{
    private EntityManager _entityManager;
    [SerializeField] private Material ground_Material;
    [SerializeField] private Mesh ground_mesh;
    // Start is called before the first frame update
    void Start()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        EntityArchetype groundArchetype = _entityManager.CreateArchetype(
            typeof(RenderMesh),
            typeof(Rotation),
            typeof(Translation),
            typeof(RenderBounds),
            typeof(LocalToWorld)
            );

        Entity ground = _entityManager.CreateEntity(groundArchetype);

        _entityManager.SetSharedComponentData(ground, new RenderMesh
        {
            material = ground_Material,
            mesh = ground_mesh,
        });

        _entityManager.SetComponentData(ground, new Translation { Value = new float3(0, 0, 0) });

        _entityManager.SetComponentData(ground, new Rotation { Value = quaternion.EulerXYZ(new float3(0, 0, 0)) });
    }
}
*/