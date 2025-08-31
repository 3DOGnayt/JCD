using System;
using UnityEngine;

namespace Core.Data
{
    [Serializable]
    public class WheelParameters
    {
        public float Mass;
        public float Radius;
        public float DampingRate;
        public float SuspensionDistance;
        public float ForceAppPointDistance;
        public Vector3 Center;

        public void SetWheelParameters(WheelCollider wheel)
        {
            wheel.mass = Mass;
            wheel.radius = Radius;
            wheel.wheelDampingRate = DampingRate;
            wheel.suspensionDistance = SuspensionDistance;
            wheel.forceAppPointDistance = ForceAppPointDistance;
            wheel.center = Center;
        }
    }
}