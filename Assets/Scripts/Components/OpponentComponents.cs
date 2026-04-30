using Scellecs.Morpeh;

namespace Components
{
    public struct OpponentSplineFollowComponent : IComponent
    {
        public float ProgressT;
        public float LookAheadMeters;
        public float TargetSpeedKmh;
        public float MaxSteerAngleDeg;
        public float BrakeLookAheadMeters;
        public float BrakeStrength;
        public float SteerErrorDegrees;
        public float ReactionDelay;
        public float SteeringDelayTimer;
        public float SteeringDelaySign;
        public float CurrentSpeedKmh;
        public bool RailInitialized;
    }
}