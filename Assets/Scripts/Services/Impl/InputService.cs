using System.Collections.Generic;
using Data;
using UnityEngine;

namespace Services.Impl
{
    public class InputService : IInputService
    {
        public void ApplySpeed_Test(CarSetup carSetup, List<WheelInfo> wheelInfos)
        {
            var motor = carSetup.MaxMotorTorque * Input.GetAxisRaw("Vertical");
            var steering = carSetup.MaxSteeringAngle * Input.GetAxisRaw("Horizontal");

            foreach (var info in wheelInfos)
            {
                if (info.Steering)
                {
                    info.LeftWheel.steerAngle = steering;
                    info.RightWheel.steerAngle = steering;
                }

                if (info.Motor)
                {
                    info.LeftWheel.motorTorque = motor;
                    info.RightWheel.motorTorque = motor;
                }

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