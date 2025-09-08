using System;
using UnityEngine;

namespace Data
{
    [Serializable]
    public class CarSetup
    {
        [Header("CAR SETUP")]
        [Space]
        [Range(0, 900)] public float MaxSpeed;
        [Range(0, 20)] public float MaxBackSpeed;
        [Range(0, 100)] public float AccelerationMultiplier;
        [Range(0, 100)] public float DecelerationMultiplier;
        [Range(0, 90)] public float MaxSteeringAngle; //TODO: take this from SteeringAngleComponent
        [Range(0, 900)] public float SteeringSpeed;
        [Range(0, 900)] public float BrakeForce;
        [Range(0, 100)] public float DriftMultiplier;
        
        public float MaxMotorTorque; //TODO: take this from MotorTorqueComponent 
        
        //TODO: this parameters -> components parameters and reverse
        //TODO: SO parameters -> components parameters and reverse
    }
}