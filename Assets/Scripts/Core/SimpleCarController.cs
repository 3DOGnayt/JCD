using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class SimpleCarController : MonoBehaviour
    {
        public List<AxleInfo> axleInfo;
        public float maxMotorTorque;
        public float maxSteeringAngle;

        private void FixedUpdate()
        {
            var motor = maxMotorTorque * Input.GetAxis("Vertical");
            var steering = maxSteeringAngle * Input.GetAxis("Horizontal");
            
            foreach (AxleInfo axleInfo in axleInfo)
            {
                if (axleInfo.Steering)
                {
                    axleInfo.LeftWheel.steerAngle = steering;
                    axleInfo.RightWheel.steerAngle = steering;
                }
                
                if (axleInfo.Motor) 
                {
                    axleInfo.LeftWheel.motorTorque = motor;
                    axleInfo.RightWheel.motorTorque = motor;
                }
                
                ApplyLocalPositionToVisuals(axleInfo.LeftWheel, axleInfo.LeftVisual);
                ApplyLocalPositionToVisuals(axleInfo.RightWheel, axleInfo.RightVisual);
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
    
    [Serializable]
    public class AxleInfo
    {
        public WheelCollider LeftWheel;
        public WheelCollider RightWheel;
        public Transform LeftVisual;
        public Transform RightVisual;
        public bool Motor;
        public bool Steering;
    }
}