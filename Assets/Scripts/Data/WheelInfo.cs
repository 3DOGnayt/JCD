using System;
using UnityEngine;

namespace Data
{
    [Serializable]
    public class WheelInfo
    {
        public WheelCollider LeftWheel;
        public WheelCollider RightWheel;
        public Transform LeftVisual;
        public Transform RightVisual;
        public bool Motor;
        public bool Steering;
    }
}