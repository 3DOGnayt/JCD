using System;

namespace Data.HelperClass
{
    [Serializable]
    public class CarSmoothingSetup
    {
        public float SpeedSmoothing = 10f;
        public float RpmSmoothing = 10f;
        public float DriftSmoothing = 10f;
        public float MaxSmoothing = 0.001f;
    }
}