namespace Configs
{
    public interface ICarMovementParameters
    {
        bool UseArcadeAssist { get; }
        float ArcadeAssistMinSpeedKmh { get; }
        float ArcadeAssistLerpSpeed  { get; }
        
        float SpeedMultiplierMax { get; }
        float SpeedMultiplierMin { get; }
        float MaxCarSpeed { get; }
        float CarMassStandard { get; }
        float SteeringSpeedMultiplierMax { get; }
        float SteeringSpeedMultiplierMin { get; }

        float MaxMotorTorque { get; }
        float EngineForwardTorque { get; }
        float EngineBackTorque { get; }
        float HandbrakeTorque { get; }
        float NeutralMaxRpm { get; }
        float AccelerationRate { get; }
        float DecelerationRate { get; }
        float MaxRpm { get; }
        float IdleRpm { get; }
        float RpmToSpeedRatio { get; }
    }
}