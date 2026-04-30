using System;
using UnityEngine;

namespace Data.HelperClass
{
    [Serializable]
    public class CarMovementHorizontalSetup
    {
        public float CarMassStandard = 1500f;
        [Space]
        public float SpeedMultiplierMax = 1f;
        public float SpeedMultiplierMin = 0.3f;
        public float SteeringSpeedMultiplierMax = 1f;
        public float SteeringSpeedMultiplierMin = 0.1f;
        public float SteeringAngleMax = 35f;
        public float SteeringSpeed = 100f;
    }
}