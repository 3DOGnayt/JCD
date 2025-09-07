using System;
using UnityEngine;

namespace Data
{
    [Serializable]
    public class MainWheelParameters
    {
        public float Mass;
        public float Radius;
        public float DampingRate;
        public float SuspensionDistance;
        public float ForceAppPointDistance;
        public Vector3 Center;
    }
}