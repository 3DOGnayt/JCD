using System;
using Data.Enums;

namespace Data.Helpers
{
    [Serializable]
    public class CarSpeedSetup
    {
        public EGear EGear;
        public int SpeedLimit;
        public int SpeedAcceleration;
        public int SpeedDecelerationNoGas; // TODO: Refactoring
        public int SpeedDecelerationBrake; // TODO: Refactoring
    }
}