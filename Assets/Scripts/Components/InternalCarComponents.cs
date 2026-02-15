using System.Collections.Generic;
using Data.HelperClass;
using Scellecs.Morpeh;
using UnityEngine;

namespace Components
{
    // CarParameters
    public struct CarMassComponent : IComponent { public float Value; }
    public struct AutomaticCenterOfMassComponent : IComponent { public bool Value; }
    public struct CenterOfMassComponent : IComponent { public Vector3 Value; }

    // MainWheelParameters
    public struct WheelInfoComponent : IComponent
    {
        public List<WheelInfoSetup> WheelInfo;
        public WheelInfoSetup FrontWheels => WheelInfo[0];
        public WheelInfoSetup BackWheels => WheelInfo[1];
    }

    //Front
    public struct FrontWheelMassComponent : IComponent { public float Value; }
    public struct FrontWheelRadiusComponent : IComponent { public float Value; }
    public struct FrontDampingRateComponent : IComponent { public float Value; }
    public struct FrontSuspensionDistanceComponent : IComponent { public float Value; }
    public struct FrontForceAppPointDistanceComponent : IComponent { public float Value; }
    public struct FrontWheelCenterComponent : IComponent { public Vector3 Value; }
    
    //Back
    public struct BackWheelMassComponent : IComponent { public float Value; }
    public struct BackWheelRadiusComponent : IComponent { public float Value; }
    public struct BackDampingRateComponent : IComponent { public float Value; }
    public struct BackSuspensionDistanceComponent : IComponent { public float Value; }
    public struct BackForceAppPointDistanceComponent : IComponent { public float Value; }
    public struct BackWheelCenterComponent : IComponent { public Vector3 Value; }
    
    // SuspensionSpringParameters
    //Front
    public struct FrontSpringComponent : IComponent { public float Value; }
    public struct FrontDamperComponent : IComponent { public float Value; }
    public struct FrontTargetPositionComponent : IComponent { public float Value; }
    
    //Back
    public struct BackSpringComponent : IComponent { public float Value; }
    public struct BackDamperComponent : IComponent { public float Value; }
    public struct BackTargetPositionComponent : IComponent { public float Value; }
    
    // ForwardAndSidewaysParameters
    //Front
    public struct FrontExtremumSlipForwardComponent : IComponent { public float Value; }
    public struct FrontExtremumValueForwardComponent : IComponent { public float Value; }
    public struct FrontAsymptoteSlipForwardComponent : IComponent { public float Value; }
    public struct FrontAsymptoteValueForwardComponent : IComponent { public float Value; }
    public struct FrontStiffnessForwardComponent : IComponent { public float Value; }
    
    public struct FrontExtremumSlipSidewaysComponent : IComponent { public float Value; }
    public struct FrontExtremumValueSidewaysComponent : IComponent { public float Value; }
    public struct FrontAsymptoteSlipSidewaysComponent : IComponent { public float Value; }
    public struct FrontAsymptoteValueSidewaysComponent : IComponent { public float Value; }
    public struct FrontStiffnessSidewaysComponent : IComponent { public float Value; }
    
    //Back
    public struct BackExtremumSlipForwardComponent : IComponent { public float Value; }
    public struct BackExtremumValueForwardComponent : IComponent { public float Value; }
    public struct BackAsymptoteSlipForwardComponent : IComponent { public float Value; }
    public struct BackAsymptoteValueForwardComponent : IComponent { public float Value; }
    public struct BackStiffnessForwardComponent : IComponent { public float Value; }
    
    public struct BackExtremumSlipSidewaysComponent : IComponent { public float Value; }
    public struct BackExtremumValueSidewaysComponent : IComponent { public float Value; }
    public struct BackAsymptoteSlipSidewaysComponent : IComponent { public float Value; }
    public struct BackAsymptoteValueSidewaysComponent : IComponent { public float Value; }
    public struct BackStiffnessSidewaysComponent : IComponent { public float Value; }
}