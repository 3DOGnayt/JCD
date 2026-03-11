namespace Configs
{
    public interface ICarMovementParameters
    {
        bool UseArcadeAssist { get; }
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
    }
}