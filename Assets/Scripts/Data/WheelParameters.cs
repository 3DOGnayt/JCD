using System;
using UnityEngine;

namespace Data
{
    [Serializable]
    public class WheelParameters
    {
        public MainWheelParameters MainWheelParameters;
        [Space]
        public SuspensionSpringParameters SuspensionSpring;
        [Space] 
        public ForwardAndSidewaysParameters ForwardFriction;
        [Space]
        public ForwardAndSidewaysParameters SidewaysFriction;

        public void SetAllParameters(WheelCollider wheel)
        {
            wheel.mass = MainWheelParameters.Mass;
            wheel.radius = MainWheelParameters.Radius;
            wheel.wheelDampingRate = MainWheelParameters.DampingRate;
            wheel.suspensionDistance = MainWheelParameters.SuspensionDistance;
            wheel.forceAppPointDistance = MainWheelParameters.ForceAppPointDistance;
            wheel.center = MainWheelParameters.Center;
            
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
            MainWheelParameters.Mass = wheel.mass;
            MainWheelParameters.Radius = wheel.radius;
            MainWheelParameters.DampingRate = wheel.wheelDampingRate;
            MainWheelParameters.SuspensionDistance = wheel.suspensionDistance;
            MainWheelParameters.ForceAppPointDistance = wheel.forceAppPointDistance;
            MainWheelParameters.Center = wheel.center;

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
            wheel.mass = MainWheelParameters.Mass;
            wheel.radius = MainWheelParameters.Radius;
            wheel.wheelDampingRate = MainWheelParameters.DampingRate;
            wheel.suspensionDistance = MainWheelParameters.SuspensionDistance;
            wheel.forceAppPointDistance = MainWheelParameters.ForceAppPointDistance;
            wheel.center = MainWheelParameters.Center;
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