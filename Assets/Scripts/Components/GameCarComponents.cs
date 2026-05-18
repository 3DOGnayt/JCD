using Data.Enums;
using Helpers.Car;
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
    public struct ShiftUpInputComponent : IComponent { public bool Value; }
    public struct ShiftDownInputComponent : IComponent { public bool Value; }
    public struct ManualGearOverrideComponent : IComponent { public float Timer; }
    public struct DriftMultiplierComponent : IComponent { public float Value; }
    public struct DownshiftDriftComponent : IComponent
    {
        public float Value;
        public float Timer;
        public float RearGripMultiplier;
        public float FrontGripMultiplier;
    }
    public struct ArcadeAssistSpeedComponent : IComponent { public float Value; }
    public struct SplineProgressComponent : IComponent
    {
        public float PrevT;
        public int Laps;
        public float Distance;
        public bool HasPrev;
    }
    public struct SplineDeltaComponent : IComponent { public float Value; }
    
    public struct SpeedMaxComponent : IComponent { public float Value; }
    public struct BackSpeedMaxComponent : IComponent { public float Value; }
    public struct GearCountComponent : IComponent { public int Value; }
    public struct EngineRpmMaxComponent : IComponent { public float Value; }
    
    public struct CarViewComponent : IComponent { public ICarView Value; }
    public struct SkidmarksComponent : IComponent { public bool Value; }
    public struct SkidAudioComponent : IComponent { public AudioSource Source; public float CurrentVolume; }
    public struct EngineAudioComponent : IComponent
    {
        public AudioSource LowSource;
        public AudioSource MedSource;
        public AudioSource HighSource;
        public float LowVolume;
        public float MedVolume;
        public float HighVolume;
    }
    public struct SkidSmokeHandleComponent : IComponent { public int Value; }
    public struct HeadlightsComponent : IComponent { public EHeadlightsMode Value; }
}
