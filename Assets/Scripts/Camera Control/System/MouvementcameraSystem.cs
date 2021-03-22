/*
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
*/
/*

#region Camera ROTATION

if (Input.GetMouseButtonDown(2)) //check if the middle mouse button was pressed
{
    camData.startPos = Input.mousePosition;
}
if (Input.GetMouseButton(2)) //check if the middle mouse button is being held down
{
    //Debug.Log($"startPosition START={camData.startPos}");
    camData.endPos = Input.mousePosition;// float3(Input.mousePosition.x, Input.mousePosition.y, 0);
    //Debug.Log($"camData.endPos START={camData.endPos}");
    //var rotationSpeed = camData.rotationSpeed;
    //var currentRotation = rotation.Value;

    float DistanceX = ((camData.endPos - camData.startPos).x) * deltaTime * camData.rotationSpeed;
    float DistanceY = ((camData.endPos - camData.startPos).y) * deltaTime * camData.rotationSpeed;


    var newCurrentRotationY = normalize(mul(GetComponent<LocalToWorld>(camera).Rotation.value, RotateY(radians(DistanceX))));
    rotation.Value = new quaternion(0, newCurrentRotationY.value.y, 0, newCurrentRotationY.value.w); //Parent rotate only horizontal
    var CurrentRotation = _entityManager.GetComponentData<LocalToWorld>(child[0].Value).Rotation.value;
    var newCurrentRotationX = normalize(mul(CurrentRotation, RotateX(radians(-DistanceY))));
    //Debug.Log($"newCurrentRotationX={newCurrentRotationX}");
    _entityManager.SetComponentData(child[0].Value, new Rotation { Value = new quaternion(newCurrentRotationX.value.x, 0, 0, newCurrentRotationX.value.w) });//child rotate only vertical
    //_entityManager.SetComponentData(child[0].Value, new Rotation { Value = newCurrentRotationX });//child rotate only vertical
    //Debug.Log($"CHILD NEW ROTATION={GetComponent<LocalToWorld>(child[0].Value).Rotation.value}");

    camData.startPos = camData.endPos;
}
#endregion Camera ROTATION
*/
/*
}).Run();

#region Camera ROTATION

var camData = GetComponent<Data_Transform_Camera>(_camera);
if (Input.GetMouseButtonDown(2)) //check if the middle mouse button was pressed
{
camData.startPos = Input.mousePosition;
// Debug.Log($"startPosition={startPosition}");
}

if (Input.GetMouseButton(2)) //check if the middle mouse button is being held down
{
camData.endPos = Input.mousePosition;
float DistanceX = (camData.endPos - camData.startPos).x * camData.rotationSpeed * Time.DeltaTime;
float DistanceY = (camData.endPos - camData.startPos).y * camData.rotationSpeed * Time.DeltaTime;

var currentRotationParent = _entityManager.GetComponentData<Rotation>(_camera).Value;
var newRotationParent = currentRotationParent * Quaternion.Euler(new Vector3(0, DistanceX, 0));
_entityManager.SetComponentData<Rotation>(_camera, new Rotation { Value = newRotationParent });
//transform.rotation *= Quaternion.Euler(new Vector3(0, DistanceX, 0));
//Debug.Log($"transform.rotation. x = {(transform.rotation * Quaternion.Euler(new Vector3(0, DistanceX, 0))).x} // y = {(transform.rotation * Quaternion.Euler(new Vector3(0, DistanceX, 0))).y} // z = {(transform.rotation * Quaternion.Euler(new Vector3(0, DistanceX, 0))).z} // w = {(transform.rotation * Quaternion.Euler(new Vector3(0, DistanceX, 0))).w}");

//var childCam = _entityManager.GetBuffer<Child>(_camera).ToNativeArray(Allocator.Temp);
var childCam = _entityManager.GetBuffer<Child>(_camera).Reinterpret<Entity>().ToNativeArray(Allocator.Temp);
var currentChildRotation = _entityManager.GetComponentData<Rotation>(childCam[0]).Value;
var newRotationChild = currentChildRotation * Quaternion.Euler(new Vector3(-DistanceY, 0, 0));
_entityManager.SetComponentData<Rotation>(_camera, new Rotation { Value = newRotationParent });
childCam.Dispose();
//transform.GetChild(0).transform.rotation *= Quaternion.Euler(new float3(-DistanceY, 0, 0));

camData.startPos = camData.endPos;

}
#endregion Camera ROTATION
}
}
*/