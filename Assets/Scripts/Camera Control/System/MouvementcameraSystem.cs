using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using static Unity.Mathematics.quaternion;

[UpdateAfter(typeof(InputCameraSystem))]
public class MouvementcameraSystem : SystemBase
{
    EntityManager _entityManager;

    protected override void OnCreate()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        RequireSingletonForUpdate<CameraTag>();
    }
    protected override void OnStartRunning()
    {
        //_maxHeight = 300f;
        //_minHeight = 3f;
    }
    protected override void OnUpdate()
    {
        bool mouseMiddle = Input.GetMouseButton(2);
        bool mouseMidleDown = Input.GetMouseButtonDown(2);
        var deltaTime = Time.DeltaTime;

        //float3 startPosition = float3(Input.mousePosition.x, Input.mousePosition.y, 0);
        //float3 EndPosition;
        var _camera = GetSingletonEntity<CameraTag>();

        Entities
            .WithAll<CameraTag>()
            .WithStructuralChanges()
            .ForEach((Entity camera, ref Data_Transform_Camera camData, ref Translation translation, ref Rotation rotation, in LocalToWorld ltw, in Data_MinHeight minHeight, in Data_MaxHeight maxHeight, in DynamicBuffer<Child> child) => 
            {
                //float2 startPosition;
                //float2 EndPosition;

                float HorizontalSpeed = deltaTime * (translation.Value.y) * camData.speed * camData.direction.x;
                float VerticalSpeed = deltaTime * (translation.Value.y) * camData.speed * camData.direction.z;
                float ScrollSpeed = deltaTime * (-camData.zoomSpeed * math.log(translation.Value.y) * camData.direction.y);
                //========================\\
                //        ZOOM PART       \\
                //========================\\
                if ((translation.Value.y >= maxHeight.value) && (ScrollSpeed > 0))
                {
                    ScrollSpeed = 0;
                }
                else if ((translation.Value.y <= minHeight.value) && (ScrollSpeed < 0))
                {
                    ScrollSpeed = 0;
                }
                if ((translation.Value.y + ScrollSpeed) > maxHeight.value)
                {
                    ScrollSpeed = maxHeight.value - translation.Value.y;
                }
                else if ((translation.Value.y + ScrollSpeed) < minHeight.value)
                {
                    ScrollSpeed = minHeight.value - translation.Value.y;
                }

                float3 VerticalMove = new float3(0, ScrollSpeed, 0);
                float3 LateralMove = HorizontalSpeed * ltw.Right;
                //Movement forward by vector projection
                float3 ForwardMove = ltw.Forward;
                ForwardMove.y = 0; //remove vertical component
                ForwardMove = normalize(ForwardMove); //normalize vector
                ForwardMove *= VerticalSpeed;

                float3 Move = VerticalMove + LateralMove + ForwardMove;

                translation.Value += Move;


                #region Camera ROTATION

                if (Input.GetMouseButtonDown(2)) //check if the middle mouse button was pressed
                {
                    camData.startPos = Input.mousePosition;
                }
                if (Input.GetMouseButton(2)) //check if the middle mouse button is being held down
                {
                    camData.endPos = Input.mousePosition;// float3(Input.mousePosition.x, Input.mousePosition.y, 0);
                    
                    var rotationSpeed = camData.rotationSpeed;
                    var currentRotation = rotation.Value;

                    float DistanceX = ((camData.endPos - camData.startPos).x) * deltaTime * camData.rotationSpeed;
                    float DistanceY = ((camData.endPos - camData.startPos).y) * deltaTime * camData.rotationSpeed;

                    //Debug.Log($"(EndPosition - startPosition).x={radians(camData.endPos - camData.startPos).x}");
                    //Use for horizontal rotation(Y Axe)
                    var newCurrentRotationY = normalize(mul(GetComponent<LocalToWorld>(camera).Rotation, RotateY(radians(DistanceX))));
                    //Use for vertical rotation(X Axe)
                    var newCurrentRotationX = normalize(mul(GetComponent<LocalToWorld>(child[0].Value).Rotation, RotateX(radians(-DistanceY))));
                    rotation.Value = new quaternion(0, newCurrentRotationY.value.y, 0, newCurrentRotationY.value.w); //Parent rotate only horizontal
                    _entityManager.SetComponentData(child[0].Value, new Rotation { Value = new quaternion(newCurrentRotationX.value.x, 0, 0, newCurrentRotationX.value.w)});//child rotate only vertical

                    camData.startPos = camData.endPos;
                }
                #endregion Camera ROTATION
            }).Run();
    }
}
