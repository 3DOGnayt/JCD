using UnityEngine;

namespace Configs
{
    public interface ICarMovementParameters
    {
        bool UseArcadeAssist { get; }
        bool UseArcadeAssistInDrift { get; }
        float ArcadeAssistMinSpeedKmh { get; }
        float ArcadeAssistLerpSpeed  { get; }
        float DriftAssistForwardSpeedMultiplier { get; }
        
        float SpeedMultiplierMax { get; }
        float SpeedMultiplierMin { get; }
        float CarMassStandard { get; }
        float SteeringSpeedMultiplierMax { get; }
        float SteeringSpeedMultiplierMin { get; }

        float HandbrakeTorque { get; }
        float AccelerationRate { get; }
        float DecelerationRate { get; }
        float MaxRpm { get; }
        float IdleRpm { get; }

        float MaxAirborneHeight { get; }
        float UprightStartAngleDeg { get; }
        float UprightTorque { get; }
        float UprightDamping { get; }
        LayerMask GroundMask { get; }
        
        bool UseVelocityAlign { get; }
        bool VelocityAlignRequireCounterSteer { get; }
        float VelocityAlignTorque { get; }
        float VelocityAlignDamping { get; }
        float VelocityAlignMinSpeedKmh { get; }
        float VelocityAlignMinSlipAngleDeg { get; }
    }
}