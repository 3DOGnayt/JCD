namespace Configs
{
    public interface ICarMovementParameters
    {
        float SpeedMultiplierMax { get; }
        float SpeedMultiplierMin { get; }
        float MaxCarSpeed { get; }
        float CarMass { get; }
        float SteeringSpeedMultiplierMax { get; }
        float SteeringSpeedMultiplierMin { get; }

        float AccelerationRate { get; }
        float DecelerationRate { get; }
        float MaxRpm { get; }
        float IdleRpm { get; }
        float RpmToSpeedRatio { get; }
    }
}