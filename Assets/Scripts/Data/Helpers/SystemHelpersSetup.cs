using System;
using UnityEngine;

namespace Data.Helpers
{
    [Serializable]
    public class SystemHelpersSetup
    {
        public float StopThresholdKmh = 0.5f;
        public float InputDeadZone = 0.05f;
        [Space]
        public float UpshiftRpmDropFactor = 0.6f;
        public float NeutralInputDeadZone = 0.05f;
        [Space]
        public float SleepSpeedThresholdMps = 0.05f;
        public float SleepAngularSpeedThreshold = 0.05f;
        [Space]
        public float MinDriftSpeedKmh = 20f;
        public float MinBrakeSkidSpeedKmh = 5f;
        public float SlipAngleThresholdDeg = 20f;
        public float DriftVisualThresh = 0.05f;
    }
}