using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
[UpdateAfter(typeof(RegimentsSystem))]
public class StartPlacementSystem : SystemBase
{
    EntityManager _entityManager;
    Entity InitPlacement;
    float3 StartPlacementCalcul;
    protected override void OnCreate()
    {
        //RequireForUpdate(GetEntityQuery(typeof(Data_StartingPlacement)));
        //RequireSingletonForUpdate<Data_StartingPlacement>();
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    protected override void OnStartRunning()
    {
        InitPlacement = GetSingletonEntity<Data_StartingPlacement>();
        var centerMesh = _entityManager.GetComponentData<Translation>(InitPlacement).Value;
        var Zoffset = _entityManager.GetComponentData<LocalToWorld>(InitPlacement).Value.c2.z / 2;
        Debug.Log(Zoffset);
        var Xoffset = _entityManager.GetComponentData<LocalToWorld>(InitPlacement).Value.c0.x / 2;
        Debug.Log(Xoffset);
        // set the spawn point to the top-left corner
        float3 RealStart = new float3(centerMesh.x - Xoffset, centerMesh.y, centerMesh.z + Zoffset);

        _entityManager.SetComponentData(InitPlacement, new Data_StartingPlacement { StartInitPlacement = RealStart });
        Debug.Log(_entityManager.GetComponentData<Data_StartingPlacement>(InitPlacement).StartInitPlacement);

        _entityManager.AddComponent<Data_Fusilier_MaxSizeFomation>(InitPlacement);
        _entityManager.SetComponentData(InitPlacement, new Data_Fusilier_MaxSizeFomation {sizeFusilier = 10});
        _entityManager.AddComponent<Data_Cavalry_MaxSizeFomation>(InitPlacement);
        _entityManager.SetComponentData(InitPlacement, new Data_Cavalry_MaxSizeFomation {sizeCavalry = 5});

    }

    protected override void OnUpdate()
    {
        this.Enabled = false;
    }
}
