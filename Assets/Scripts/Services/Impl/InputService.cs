using System.Collections.Generic;
using Data.HelperClass;
using UnityEngine;

namespace Services.Impl
{
    public class InputService : IInputService
    {
        public void ApplyHorizontalMove(float targetAngle, float steeringSpeed, List<WheelInfoSetup> wheelInfos)
        {
            foreach (var info in wheelInfos)
            {
                if (!info.Steering)
                    continue;

                var left = Mathf.MoveTowards(
                    info.LeftWheel.steerAngle,
                    targetAngle,
                    steeringSpeed * Time.fixedDeltaTime);

                var right = Mathf.MoveTowards(
                    info.RightWheel.steerAngle,
                    targetAngle,
                    steeringSpeed * Time.fixedDeltaTime);
                
                info.LeftWheel.steerAngle = left;
                info.RightWheel.steerAngle = right;

                ApplyLocalPositionToVisuals(info.LeftWheel, info.LeftVisual);
                ApplyLocalPositionToVisuals(info.RightWheel, info.RightVisual);
            }
        }

        public void ApplyVerticalMove(float currentMotorTorque, float input, List<WheelInfoSetup> wheelInfos)
        {
            var torque = currentMotorTorque * input;
            //var torque = currentMotorTorque * Mathf.Sign(input);
            
            foreach (var info in wheelInfos)
            {
                if (!info.Motor) 
                    continue;
                
                info.LeftWheel.motorTorque = torque;
                info.RightWheel.motorTorque = torque;
                
                ApplyLocalPositionToVisuals(info.LeftWheel, info.LeftVisual);
                ApplyLocalPositionToVisuals(info.RightWheel, info.RightVisual);
            }
        }

        private void ApplyLocalPositionToVisuals(WheelCollider wheelCollider, Transform visualWheel)
        {
            Vector3 position;
            Quaternion rotation;
            wheelCollider.GetWorldPose(out position, out rotation);

            visualWheel.position = position;
            visualWheel.rotation = rotation;
        }
    }
}