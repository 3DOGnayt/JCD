using Scellecs.Morpeh;

namespace Components
{
    public struct SpeedComponent : IComponent { public float Value; }
    public struct CurrentGearComponent : IComponent { public int Value; }
    public struct GearCountComponent : IComponent { public int Value; }
    
    public struct BackSpeedComponent : IComponent { public float Value; }
    public struct MotorTorqueComponent : IComponent { public float Value; }
    public struct AccelerationMultiplierComponent : IComponent { public float Value; }
    public struct DecelerationMultiplierComponent : IComponent { public float Value; }
    public struct SteeringAngleComponent : IComponent { public float Value; }
    public struct SteeringSpeedComponent : IComponent { public float Value; }
    public struct BrakeForceComponent : IComponent { public float Value; }
    public struct DriftMultiplierComponent : IComponent { public float Value; }
}