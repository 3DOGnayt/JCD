using System;
using Data.Enums;
using UnityEngine;

namespace Data.HelperClass
{
    [Serializable]
    public class CarSpeedSetup
    {
        public EGear EGear;
        public int SpeedLimit;
        public int SpeedAcceleration;
        [Space]
        public int RpmMin;
        public int RpmMax;
    }
}