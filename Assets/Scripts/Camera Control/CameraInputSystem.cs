using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using System;

public class CameraInputSystem : SystemBase
{
    protected override void OnCreate()
    {
        base.OnCreate();
    }
    protected override void OnUpdate()
    {

        Entities
            .WithoutBurst()
            .ForEach((ref DataMove_Camera moveCamera ,in DataInput_Camera inputCamera) => {
                bool isRightKeyPressed = Input.GetKey(inputCamera.rightKey);
                bool isLeftKeyPressed = Input.GetKey(inputCamera.leftKey);
                bool isUpKeyPressed = Input.GetKey(inputCamera.upKey);
                bool isDownKeyPressed = Input.GetKey(inputCamera.downKey);
                //bool leftShiftPressed = Input.GetKey(inputCamera.leftShift);
                /*
                bool isScrolling = Input.mouseScrollDelta.Equals(float2.zero) ? false : true;
                if(isScrolling)
                {
                    bool IsScrollDown = Input.mouseScrollDelta.y < 0 ? true : false;
                    bool IsScrollUp = Input.mouseScrollDelta.y > 0 ? true : false;
                }
                */
                moveCamera.direction.x = (isRightKeyPressed) ? 1 : 0;
                moveCamera.direction.x -= (isLeftKeyPressed) ? 1 : 0;
                moveCamera.direction.z = (isUpKeyPressed) ? 1 : 0;
                moveCamera.direction.z -= (isDownKeyPressed) ? 1 : 0;
                //moveCamera.direction.y = (IsScrollDown) ? 1 : 0;
                //moveCamera.direction.y -= (IsScrollDown) ? 1 : 0;

            }).Run();
    }
}
