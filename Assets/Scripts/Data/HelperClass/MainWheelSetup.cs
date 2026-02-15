using System;
using UnityEngine;

namespace Data.HelperClass
{
    [Serializable]
    public class MainWheelSetup
    {
        public float Mass;
        public float Radius;
        public float DampingRate;
        public float SuspensionDistance;
        public float ForceAppPointDistance;
        public Vector3 Center;
    }
}