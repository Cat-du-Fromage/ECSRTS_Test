using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
/*
public class CameraSystem : SystemBase
{
    Entity _camera;

    EntityManager _entityManager;
    float _speed;
    float _zoomSpeed;
    float _rotationSpeed;

    float _maxHeight = 300f;
    float _minHeight = 3f;

    float2 startPosition;
    float2 EndPosition;
    protected override void OnCreate()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        RequireSingletonForUpdate<CameraTag>();
    }
    protected override void OnStartRunning()
    {
        _camera = GetSingletonEntity<CameraTag>();
        _rotationSpeed = 20f;
    }
    protected override void OnUpdate()
    {
        float _camPosY = GetComponent<Translation>(_camera).Value.y;

        _speed = Input.GetKey(KeyCode.LeftShift) ? 3.6f : 1.8f;
        _zoomSpeed = Input.GetKey(KeyCode.LeftShift) ? 1080.0f : 540.0f;

        // "Mathf.Log(transform.position.y)" Adjust the speed the higher the camera is
        float HorizontalSpeed = Time.DeltaTime * (_camPosY) * _speed * Input.GetAxis("Horizontal");
        float VerticalSpeed = Time.DeltaTime * (_camPosY) * _speed * Input.GetAxis("Vertical");
        float ScrollSpeed = Time.DeltaTime * (-_zoomSpeed * math.log(_camPosY) * Input.GetAxis("Mouse ScrollWheel"));
        //========================\\
        //        ZOOM PART       \\
        //========================\\
        if ((_camPosY >= _maxHeight) && (ScrollSpeed > 0))
        {
            ScrollSpeed = 0;
        }
        else if ((_camPosY <= _minHeight) && (ScrollSpeed < 0))
        {
            ScrollSpeed = 0;
        }
        if ((_camPosY + ScrollSpeed) > _maxHeight)
        {
            ScrollSpeed = _maxHeight - _camPosY;
        }
        else if ((_camPosY + ScrollSpeed) < _minHeight)
        {
            ScrollSpeed = _minHeight - _camPosY;
        }

        float3 VerticalMove = new float3(0, ScrollSpeed, 0);
        float3 LateralMove = HorizontalSpeed * GetComponent<LocalToWorld>(_camera).Right;
        //Movement forward by vector projection
        float3 ForwardMove = GetComponent<LocalToWorld>(_camera).Forward;
        ForwardMove.y = 0; //remove vertical component
        math.normalize(ForwardMove); //normalize vector
        ForwardMove *= VerticalSpeed;

        float3 Move = VerticalMove + LateralMove + ForwardMove;
        var posCam = GetComponent<Translation>(_camera).Value;
        SetComponent(_camera, new Translation { Value = posCam + Move });

        Entities.ForEach((ref Translation translation, in Rotation rotation) => {
            // Implement the work to perform for each entity here.
            // You should only access data that is local or that is a
            // field on this job. Note that the 'rotation' parameter is
            // marked as 'in', which means it cannot be modified,
            // but allows this job to run in parallel with other jobs
            // that want to read Rotation component data.
            // For example,
            //     translation.Value += math.mul(rotation.Value, new float3(0, 0, 1)) * deltaTime;
        }).Schedule();
    }
}
*/