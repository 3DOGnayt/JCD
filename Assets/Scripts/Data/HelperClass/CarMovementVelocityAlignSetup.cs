using System;

namespace Data.HelperClass
{
    [Serializable]
    public class CarMovementVelocityAlignSetup
    {
        public bool UseVelocityAlign = true;
        public bool VelocityAlignRequireCounterSteer = true;
        public float VelocityAlignTorque = 10f;
        public float VelocityAlignDamping = 2f;
        public float VelocityAlignMinSpeedKmh = 10f;
        public float VelocityAlignMinSlipAngleDeg = 10f;
    }
}
