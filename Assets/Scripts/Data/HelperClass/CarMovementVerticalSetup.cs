using System;

namespace Data.HelperClass
{
    [Serializable]
    public class CarMovementVerticalSetup
    {
        public float HandbrakeTorque;
        public float BrakeTorque;
        public float AccelerationRate; // visual on speedometer
        public float DecelerationRate; // visual on speedometer
        public float MaxRpm;
        public float IdleRpm;
    }
}