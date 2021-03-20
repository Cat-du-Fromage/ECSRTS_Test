using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class InputCameraSystem : SystemBase
{
    EntityManager _entityManager;
    protected override void OnCreate()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        RequireForUpdate(GetEntityQuery(ComponentType.ReadOnly<CameraTag>()));
    }
    protected override void OnStartRunning()
    {
        Entity _camera = GetSingletonEntity<CameraTag>();

        _entityManager.AddComponent<Data_Input_Camera>(GetEntityQuery(ComponentType.ReadOnly<CameraTag>()));
        _entityManager.AddComponent<Data_Transform_Camera>(GetEntityQuery(ComponentType.ReadOnly<CameraTag>()));
        _entityManager.AddComponents(_camera, new ComponentTypes(typeof(Data_MinHeight), typeof(Data_MaxHeight)));

        _entityManager.SetComponentData(_camera, new Data_MinHeight { value = 3f });
        _entityManager.SetComponentData(_camera, new Data_MaxHeight { value = 300f });

        _entityManager.SetComponentData(_camera, new Data_Transform_Camera { startPos = new float3(0,0,0) });
        _entityManager.SetComponentData(_camera, new Data_Transform_Camera { endPos = new float3(0, 0, 0) });

        Data_Input_Camera dataInputCam = new Data_Input_Camera();
        dataInputCam.leftShift = KeyCode.LeftShift;
        dataInputCam.mouseMiddle = KeyCode.Mouse2;
        _entityManager.SetComponentData(GetSingletonEntity<CameraTag>(), dataInputCam);
    }
    protected override void OnUpdate()
    {
        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");
        var mouseWheel = Input.GetAxis("Mouse ScrollWheel");

        Entities
            .WithName("GatherInput")
            .WithAll<CameraTag>()
            .ForEach((Entity cam, ref Data_Input_Camera inputData, ref Data_Transform_Camera camTransform) =>
            {
                inputData.MoveAxis = new float3(horizontal, mouseWheel, vertical);
                //inputData.ZoomAxis = mouseWheel;

                bool isLefShiftPressed = Input.GetKey(inputData.leftShift);
                camTransform.rotationSpeed = 20f;
                camTransform.speed = isLefShiftPressed ? 3.6f : 1.8f;
                camTransform.zoomSpeed = isLefShiftPressed ? 1080.0f : 540.0f;

                camTransform.direction.x = inputData.MoveAxis.x;
                camTransform.direction.z = inputData.MoveAxis.z;
                camTransform.direction.y = inputData.MoveAxis.y;


            }).Run();
    }
}
