using System;
using Data.Enums;

namespace Data.HelperClass
{
    [Serializable]
    public class CarSpeedSetup
    {
        public EGear EGear;
        public int SpeedLimit;
        public int SpeedAcceleration;
        public int SpeedDecelerationNoGas; // TODO: CHANGE OR DELETE
        public int SpeedDecelerationBrake; // TODO: CHANGE OR DELETE
    }
}