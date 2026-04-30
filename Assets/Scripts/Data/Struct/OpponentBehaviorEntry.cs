using System;

namespace Data.Struct
{
    [Serializable]
    public struct OpponentBehaviorEntry
    {
        public float TargetSpeedKmh;
        public float LookAheadMeters;
        public float BrakeLookAheadMeters;
        public float MaxSteerAngleDeg;
        public float SteerErrorDegrees;
        public float BrakeStrength;
        public float ReactionDelay;
    }
}