using System;
using UnityEngine;

namespace Data.HelperClass
{
    [Serializable]
    public class CarSetup
    {
        [Header("CAR SETUP")]
        [Space]
        [Range(0, 900)] public float SpeedMax;
        [Range(0, 200)] public float BackSpeedMax;
        [Range(0, 10)] public int GearCount;
        [Range(0, 12000)] public float EngineRpmMax;
        [Space]
        [Range(0, 90)] public float SteeringAngleMax;
        [Range(0, 900)] public float SteeringSpeed;
        [Space]
        public bool BrakeInput;
        public bool HandbrakeInput;
        [Space]
        [Range(0, 100)] public float DriftMultiplier;
    }
}