/*
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(InputCameraSystem))]
public class CameraSystem : SystemBase
{
    Entity _camera;

    EntityManager _entityManager;

    float _maxHeight = 300f;
    float _minHeight = 5f;

    float3 startPosition;
    float3 EndPosition;

    protected override void OnCreate()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        RequireSingletonForUpdate<CameraTag>();
    }
    protected override void OnStartRunning()
    {
        _camera = GetSingletonEntity<CameraTag>();
    }

    protected override void OnUpdate()
    {
        var camData = GetComponent<Data_Transform_Camera>(_camera);
        var deltaTime = Time.DeltaTime;
        var translation = GetComponent<Translation>(_camera);
        var ltw = GetComponent<LocalToWorld>(_camera);

        float HorizontalSpeed = deltaTime * (translation.Value.y) * camData.speed * camData.direction.x;
        float VerticalSpeed = deltaTime * (translation.Value.y) * camData.speed * camData.direction.z;
        float ScrollSpeed = deltaTime * (-camData.zoomSpeed * math.log(translation.Value.y) * camData.direction.y);
        //========================\\
        //        ZOOM PART       \\
        //========================\\
        if ((translation.Value.y >= _maxHeight) && (ScrollSpeed > 0))
        {
            ScrollSpeed = 0;
        }
        else if ((translation.Value.y <= _minHeight) && (ScrollSpeed < 0))
        {
            ScrollSpeed = 0;
        }
        if ((translation.Value.y + ScrollSpeed) > _maxHeight)
        {
            ScrollSpeed = _maxHeight - translation.Value.y;
        }
        else if ((translation.Value.y + ScrollSpeed) < _minHeight)
        {
            ScrollSpeed = _minHeight - translation.Value.y;
        }

        float3 VerticalMove = new float3(0, ScrollSpeed, 0);
        float3 LateralMove = HorizontalSpeed * ltw.Right;
        //Movement forward by vector projection
        float3 ForwardMove = ltw.Forward;
        ForwardMove.y = 0; //remove vertical component
        ForwardMove = math.normalize(ForwardMove); //normalize vector
        ForwardMove *= VerticalSpeed;

        float3 Move = VerticalMove + LateralMove + ForwardMove;
        var posCam = GetComponent<Translation>(_camera).Value;
        SetComponent(_camera, new Translation { Value = posCam + Move });

        #region Camera ROTATION

        //var camData = GetComponent<Data_Transform_Camera>(_camera);
        if (Input.GetMouseButtonDown(2)) //check if the middle mouse button was pressed
        {
            startPosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(2)) //check if the middle mouse button is being held down
        {
            EndPosition = Input.mousePosition;
            float DistanceX = (EndPosition - startPosition).x * camData.rotationSpeed * Time.DeltaTime;
            float DistanceY = (EndPosition - startPosition).y * camData.rotationSpeed * Time.DeltaTime;

            var currentRotationParent = _entityManager.GetComponentData<Rotation>(_camera).Value;
            var newRotationParent = currentRotationParent * Quaternion.Euler(new Vector3(0, DistanceX, 0));
            _entityManager.SetComponentData<Rotation>(_camera, new Rotation { Value = newRotationParent });

            var childCam = _entityManager.GetBuffer<Child>(_camera).Reinterpret<Entity>().ToNativeArray(Allocator.Temp);
            var currentChildRotation = _entityManager.GetComponentData<Rotation>(childCam[0]).Value;
            var newRotationChild = currentChildRotation * Quaternion.Euler(new Vector3(-DistanceY, 0, 0));

            _entityManager.SetComponentData<Rotation>(childCam[0], new Rotation { Value = newRotationChild });
            childCam.Dispose();
            startPosition = EndPosition;

        }
        #endregion Camera ROTATION
    }
}
*/