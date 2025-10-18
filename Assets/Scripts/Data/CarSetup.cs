using System;
using UnityEngine;

namespace Data
{
    [Serializable]
    public class CarSetup
    {
        [Header("CAR SETUP")]
        [Space]
        [Range(0, 900)] public float CurrentSpeed;
        [Range(-1, 10)] public int CurrentGear;
        [Range(0, 10)] public int GearCount;
        [Range(0, 200)] public float CurrentBackSpeed;
        [Range(0, 100)] public float AccelerationMultiplier;
        [Range(0, 100)] public float DecelerationMultiplier;
        [Range(0, 90)] public float CurrentSteeringAngle;
        [Range(0, 900)] public float SteeringSpeed;
        [Range(0, 900)] public float BrakeForce;
        [Range(0, 100)] public float DriftMultiplier;
        
        public float CurrentMotorTorque;
    }
}