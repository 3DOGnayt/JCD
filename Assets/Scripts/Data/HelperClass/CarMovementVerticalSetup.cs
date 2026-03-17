using System;

namespace Data.HelperClass
{
    [Serializable]
    public class CarMovementVerticalSetup
    {
        public float HandbrakeTorque;
        public float BrakeTorque;
        public float AccelerationRate;
        public float DecelerationRate;
        public float MaxRpm;
        public float IdleRpm;
    }
}