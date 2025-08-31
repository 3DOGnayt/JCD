using System.Collections.Generic;
using Core.Data;
using UnityEngine;

namespace Core.Moving
{
    public class CarController : MonoBehaviour
    {
        [Header("CAR SETUP")]
        [Space]
        [Range(0, 900)]
        public float MaxSpeed;
        [Range(0, 20)]
        public float MaxBackSpeed;
        [Range(0, 100)]
        public float AccelerationMultiplier;
        [Range(0, 90)]
        public float MaxSteeringAngle;
        [Range(0, 900)]
        public float SteeringSpeed;
        [Range(0, 900)]
        public float BrakeForce;
        [Range(0, 100)]
        public float DecelerationMultiplier;
        [Range(0, 100)]
        public float DriftMultiplier;
        
        public float MaxMotorTorque;
        [Space]
        [Header("CAR RESOURCES")]
        [Space]
        public Rigidbody CarRigidbody;
        [Space]
        [Header("WHEELS")]
        [Space]
        public List<AxleInfo> axleInfo;
        public CarParameters _carParameters;
        public WheelParameters _wheelParameters;
        public WheelSubParameters _wheelSubParameters;

        private void FixedUpdate()
        {
            ApplySpeed_Test();
        }

        private void ApplySpeed_Test()
        {
            var motor = MaxMotorTorque * Input.GetAxisRaw("Vertical");
            var steering = MaxSteeringAngle * Input.GetAxisRaw("Horizontal");

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