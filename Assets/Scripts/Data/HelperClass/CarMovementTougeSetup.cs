using System;
using UnityEngine;

namespace Data.HelperClass
{
    [Serializable]
    public class CarMovementTougeSetup
    {
        public bool UseTougeHybridControl;
        public bool DisableHandbrakeDrift = true;

        [Header("Transmission")]
        public bool UseAutomaticGearShift;
        public float ManualGearOverrideSeconds = 0.75f;

        [Header("Input")]
        public KeyCode ShiftUpKey = KeyCode.E;
        public KeyCode ShiftDownKey = KeyCode.Q;

        [Header("Downshift Drift")]
        public float MinDownshiftDriftSpeedKmh = 45f;
        [Range(0f, 1f)] public float MinDownshiftDriftSteer = 0.25f;
        [Range(0f, 1f)] public float RearGripOnDownshift = 0.55f;
        [Range(0f, 1f)] public float FrontGripOnDownshift = 0.95f;
        public float DriftSecondsOnDownshift = 0.35f;
        public float DriftReturnSpeed = 1.8f;
        public float YawKickOnDownshift = 0.2f;
        [Range(0f, 1f)] public float DriftHoldFromThrottle = 0.65f;
    }
}
