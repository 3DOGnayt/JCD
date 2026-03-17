using System;
using UnityEngine;

namespace Data.HelperClass
{
    [Serializable]
    public class CarMovementAirControlSetup
    {
        public float MaxAirborneHeight = 0.5f;
        public float UprightStartAngleDeg = 10f;
        public float UprightTorque = 8f;
        public float UprightDamping = 1.5f;
        public LayerMask GroundMask = ~0;
    }
}