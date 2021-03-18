using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class CameraControlSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;
        Entities
            .WithoutBurst()
            .WithAll<CameraTag>()
            .ForEach((ref Translation posCamera, ref DataMove_Camera moveData, in DataInput_Camera datainput) => 
            {
                moveData._speed = Input.GetKey(KeyCode.LeftShift) ? 3.6f : 1.8f;
                moveData._zoomSpeed = Input.GetKey(KeyCode.LeftShift) ? 1080.0f : 540.0f;



                float3 DirNormalize = math.normalizesafe(moveData.direction);
                // "Mathf.Log(transform.position.y)" Adjust the speed the higher the camera is
                float HorizontalSpeed = Time.DeltaTime * (posCamera.Value.y) * moveData._speed;
                float VerticalSpeed = Time.DeltaTime * (posCamera.Value.y) * moveData._speed;
                float ScrollSpeed = Time.DeltaTime * (-moveData._zoomSpeed * math.log(posCamera.Value.y));

                //posCamera += deltaTime * DirNormalize * HorizontalSpeed;
            }).Run();
    }
}
