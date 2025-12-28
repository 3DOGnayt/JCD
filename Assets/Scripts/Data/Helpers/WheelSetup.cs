using System;
using UnityEngine;

namespace Data.Helpers
{
    [Serializable]
    public class WheelSetup
    {
        public MainWheelSetup mainWheelSetup;
        [Space]
        public SuspensionSpringSetup SuspensionSpring;
        [Space] 
        public ForwardAndSidewaysSetup ForwardFriction;
        [Space]
        public ForwardAndSidewaysSetup SidewaysFriction;

        public void SetAllParameters(WheelCollider wheel)
        {
            wheel.mass = mainWheelSetup.Mass;
            wheel.radius = mainWheelSetup.Radius;
            wheel.wheelDampingRate = mainWheelSetup.DampingRate;
            wheel.suspensionDistance = mainWheelSetup.SuspensionDistance;
            wheel.forceAppPointDistance = mainWheelSetup.ForceAppPointDistance;
            wheel.center = mainWheelSetup.Center;
            
            var suspensionSpring = wheel.suspensionSpring;
            suspensionSpring.spring = SuspensionSpring.Spring;
            suspensionSpring.damper = SuspensionSpring.Damper;
            suspensionSpring.targetPosition = SuspensionSpring.TargetPosition;
            wheel.suspensionSpring = suspensionSpring;
            
            var forwardFriction = wheel.forwardFriction;
            forwardFriction.extremumSlip = ForwardFriction.ExtremumSlip;
            forwardFriction.extremumValue = ForwardFriction.ExtremumValue;
            forwardFriction.asymptoteSlip = ForwardFriction.AsymptoteSlip;
            forwardFriction.asymptoteValue = ForwardFriction.AsymptoteValue;
            forwardFriction.stiffness = ForwardFriction.Stiffness;
            wheel.forwardFriction = forwardFriction;
            
            var sidewaysFriction = wheel.sidewaysFriction;
            sidewaysFriction.extremumSlip = SidewaysFriction.ExtremumSlip;
            sidewaysFriction.extremumValue = SidewaysFriction.ExtremumValue;
            sidewaysFriction.asymptoteSlip = SidewaysFriction.AsymptoteSlip;
            sidewaysFriction.asymptoteValue = SidewaysFriction.AsymptoteValue;
            sidewaysFriction.stiffness = SidewaysFriction.Stiffness;
            wheel.sidewaysFriction = sidewaysFriction;
        }
        
        public void SaveFromWheel(WheelCollider wheel)
        {
            mainWheelSetup.Mass = wheel.mass;
            mainWheelSetup.Radius = wheel.radius;
            mainWheelSetup.DampingRate = wheel.wheelDampingRate;
            mainWheelSetup.SuspensionDistance = wheel.suspensionDistance;
            mainWheelSetup.ForceAppPointDistance = wheel.forceAppPointDistance;
            mainWheelSetup.Center = wheel.center;

            var spring = wheel.suspensionSpring;
            SuspensionSpring.Spring = spring.spring;
            SuspensionSpring.Damper = spring.damper;
            SuspensionSpring.TargetPosition = spring.targetPosition;

            var frontFriction = wheel.forwardFriction;
            ForwardFriction.ExtremumSlip = frontFriction.extremumSlip;
            ForwardFriction.ExtremumValue = frontFriction.extremumValue;
            ForwardFriction.AsymptoteSlip = frontFriction.asymptoteSlip;
            ForwardFriction.AsymptoteValue = frontFriction.asymptoteValue;
            ForwardFriction.Stiffness = frontFriction.stiffness;

            var backFriction = wheel.sidewaysFriction;
            SidewaysFriction.ExtremumSlip = backFriction.extremumSlip;
            SidewaysFriction.ExtremumValue = backFriction.extremumValue;
            SidewaysFriction.AsymptoteSlip = backFriction.asymptoteSlip;
            SidewaysFriction.AsymptoteValue = backFriction.asymptoteValue;
            SidewaysFriction.Stiffness = backFriction.stiffness;
        }

        public void SetMainParameters(WheelCollider wheel)
        {
            wheel.mass = mainWheelSetup.Mass;
            wheel.radius = mainWheelSetup.Radius;
            wheel.wheelDampingRate = mainWheelSetup.DampingRate;
            wheel.suspensionDistance = mainWheelSetup.SuspensionDistance;
            wheel.forceAppPointDistance = mainWheelSetup.ForceAppPointDistance;
            wheel.center = mainWheelSetup.Center;
        }

        public void SetSuspensionParameters(WheelCollider wheel)
        {
            var suspensionSpring = wheel.suspensionSpring;
            suspensionSpring.spring = SuspensionSpring.Spring;
            suspensionSpring.damper = SuspensionSpring.Damper;
            suspensionSpring.targetPosition = SuspensionSpring.TargetPosition;
            wheel.suspensionSpring = suspensionSpring;
        }
        
        public void SetForwardFrictionParameters(WheelCollider wheel)
        {
            var forwardFriction = wheel.forwardFriction;
            forwardFriction.extremumSlip = ForwardFriction.ExtremumSlip;
            forwardFriction.extremumValue = ForwardFriction.ExtremumValue;
            forwardFriction.asymptoteSlip = ForwardFriction.AsymptoteSlip;
            forwardFriction.asymptoteValue = ForwardFriction.AsymptoteValue;
            forwardFriction.stiffness = ForwardFriction.Stiffness;
            wheel.forwardFriction = forwardFriction;
        }
        
        public void SetSidewaysFrictionParameters(WheelCollider wheel)
        {
            var sidewaysFriction = wheel.sidewaysFriction;
            sidewaysFriction.extremumSlip = SidewaysFriction.ExtremumSlip;
            sidewaysFriction.extremumValue = SidewaysFriction.ExtremumValue;
            sidewaysFriction.asymptoteSlip = SidewaysFriction.AsymptoteSlip;
            sidewaysFriction.asymptoteValue = SidewaysFriction.AsymptoteValue;
            sidewaysFriction.stiffness = SidewaysFriction.Stiffness;
            wheel.sidewaysFriction = sidewaysFriction;
        }
    }
}