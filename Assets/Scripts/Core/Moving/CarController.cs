using System.Collections.Generic;
using Core.Data;
using UnityEngine;

namespace Core.Moving
{
    public class CarController : MonoBehaviour
    {
        public List<AxleInfo> axleInfo;
        public float maxMotorTorque;
        public float maxSteeringAngle;

        private void FixedUpdate()
        {
            var motor = maxMotorTorque * Input.GetAxis("Vertical");
            var steering = maxSteeringAngle * Input.GetAxis("Horizontal");
            
            foreach (var info in axleInfo)
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