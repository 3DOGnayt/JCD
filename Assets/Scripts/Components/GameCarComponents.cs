using Data.Enums;
using Helpers.CarView;
using Scellecs.Morpeh;
using UnityEngine;

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
    public struct SkidmarksComponent : IComponent { public bool Value; }
    public struct SkidAudioComponent : IComponent { public AudioSource Source; public float CurrentVolume; }
    public struct SkidSmokeHandleComponent : IComponent { public int Value; }
    public struct HeadlightsComponent : IComponent { public EHeadlightsMode Value; }
}