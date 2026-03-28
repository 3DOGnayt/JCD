using System;

namespace Data.Struct
{
    [Serializable]
    public struct OpponentBehaviorEntry
    {
        public float TargetSpeedKmh;
        public float LookAheadMeters;
        public float MaxSteerAngleDeg;
        public float BrakeLookAheadMeters;
        public float BrakeStrength;
        public float SteerErrorDegrees;
        public float ReactionDelay;
    }
}