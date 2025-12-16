using Core;
using Scellecs.Morpeh;

namespace Components
{
    public struct SpeedComponent : IComponent { public float Value; }
    public struct GearComponent : IComponent { public int Value; }
    
    public struct BackSpeedComponent : IComponent { public float Value; }
    public struct EngineRpmComponent : IComponent { public float Value; }
    public struct SteeringAngleComponent : IComponent { public float Value; }
    public struct SteeringSpeedComponent : IComponent { public float Value; }
    public struct BrakeInputComponent : IComponent { public bool Value; }
    public struct HandbrakeInputComponent : IComponent { public bool  Value; }
    public struct DriftMultiplierComponent : IComponent { public float Value; }
    
    public struct SpeedMaxComponent : IComponent { public float Value; }
    public struct BackSpeedMaxComponent : IComponent { public float Value; }
    public struct GearCountComponent : IComponent { public int Value; }
    public struct EngineRpmMaxComponent : IComponent { public float Value; }
    
    public struct CarViewComponent : IComponent { public ICarView Value; }
    public struct DriftComponent : IComponent { public bool Value; }
}