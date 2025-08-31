using System;

namespace Core.Data
{
    [Serializable]
    public class ForwardAndSidewaysParameters
    {
        public float ExtremumSlip;
        public float ExtremumValue;
        public float AsymptoteSlip;
        public float AsymptoteValue;
        public float Stiffness;
    }
}