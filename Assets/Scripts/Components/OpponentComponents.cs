using Scellecs.Morpeh;

namespace Components
{
    public struct OpponentSplineFollowComponent : IComponent
    {
        public float ProgressT;
        public float LookAheadMeters;
        public float TargetSpeedKmh;
        public float MaxSteerAngleDeg;
    }
}