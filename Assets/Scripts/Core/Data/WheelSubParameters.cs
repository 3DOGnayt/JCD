using System;
using UnityEngine;

namespace Core.Data
{
    [Serializable]
    public class WheelSubParameters
    {
        public float Spring;
        public float Damper;
        public float TargetPosition;
        [Space] 
        public ForwardAndSidewaysParameters ForwardFriction;
        public ForwardAndSidewaysParameters SidewaysFriction;

        public void SetParameters(WheelCollider wheel)
        {
            var suspensionSpring = wheel.suspensionSpring;
            suspensionSpring.spring = Spring;
            suspensionSpring.damper = Damper;
            suspensionSpring.targetPosition = TargetPosition;
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
        
        public void SetSuspensionParameters(WheelCollider wheel)
        {
            var suspensionSpring = wheel.suspensionSpring;
            suspensionSpring.spring = Spring;
            suspensionSpring.damper = Damper;
            suspensionSpring.targetPosition = TargetPosition;
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