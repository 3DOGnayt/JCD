using System;

namespace Data.Helpers
{
    [Serializable]
    public class CarSpeedSettings
    {
        public EGear EGear;
        public int SpeedLimit;
        public int SpeedAcceleration;
        public int SpeedDecelerationNoGas;
        public int SpeedDecelerationBrake;
    }
}