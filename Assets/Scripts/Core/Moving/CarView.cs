using System.Collections.Generic;
using Configs.Impl;
using Data;
using UnityEngine;

namespace Core.Moving
{
    public class CarView : MonoBehaviour
    {
        [SerializeField] private CarPreset _carPreset;
        [Space]
        [SerializeField] private CarSetup _carSetup;
        [Space]
        public List<WheelInfo> _wheelInfos;

        private void FixedUpdate()
        {
            ApplySpeed_Test();
        }

        private void ApplySpeed_Test()
        {
            var motor = _carSetup.MaxMotorTorque * Input.GetAxisRaw("Vertical");
            var steering = _carSetup.MaxSteeringAngle * Input.GetAxisRaw("Horizontal");

            foreach (var info in _wheelInfos)
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