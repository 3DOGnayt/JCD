using System;
using UnityEngine;

namespace Data
{
    [Serializable]
    public class WheelParameters
    {
        public float Mass;
        public float Radius;
        public float DampingRate;
        public float SuspensionDistance;
        public float ForceAppPointDistance;
        public Vector3 Center;
        [Space]
        public SuspensionSpringParameters SuspensionSpring;
        [Space] 
        public ForwardAndSidewaysParameters ForwardFriction;
        [Space]
        public ForwardAndSidewaysParameters SidewaysFriction;

        public void SetAllParameters(WheelCollider wheel)
        {
            wheel.mass = Mass;
            wheel.radius = Radius;
            wheel.wheelDampingRate = DampingRate;
            wheel.suspensionDistance = SuspensionDistance;
            wheel.forceAppPointDistance = ForceAppPointDistance;
            wheel.center = Center;
            
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

        public void SetMainParameters(WheelCollider wheel)
        {
            wheel.mass = Mass;
            wheel.radius = Radius;
            wheel.wheelDampingRate = DampingRate;
            wheel.suspensionDistance = SuspensionDistance;
            wheel.forceAppPointDistance = ForceAppPointDistance;
            wheel.center = Center;
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