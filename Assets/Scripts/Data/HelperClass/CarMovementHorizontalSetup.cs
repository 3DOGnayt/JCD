using System;

namespace Data.HelperClass
{
    [Serializable]
    public class CarMovementHorizontalSetup
    {
        public float SpeedMultiplierMax = 1f;
        public float SpeedMultiplierMin = 0.3f;
        public float CarMassStandard = 1500f;
        public float SteeringSpeedMultiplierMax = 1f;
        public float SteeringSpeedMultiplierMin = 0.1f;
    }
}