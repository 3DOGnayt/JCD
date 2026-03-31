using Components;
using UnityEngine;

namespace Systems.Car
{
    public sealed partial class OpponentAISystem
    {
        private float ApplySteeringDelay(float desiredSteer, ref OpponentSplineFollowComponent follow, float deltaTime)
        {
            if (Mathf.Abs(desiredSteer) <= 0f)
            {
                follow.SteeringDelayTimer = 0f;
                follow.SteeringDelaySign = 0f;
                return 0f;
            }

            var desiredSign = Mathf.Sign(desiredSteer);
            if (!Mathf.Approximately(follow.SteeringDelaySign, desiredSign))
            {
                follow.SteeringDelaySign = desiredSign;
                follow.SteeringDelayTimer = 0f;
            }

            var delay = Mathf.Max(0f, follow.ReactionDelay);
            if (delay <= 0f)
                return desiredSteer;

            follow.SteeringDelayTimer += deltaTime;
            if (follow.SteeringDelayTimer < delay)
                return 0f;

            return desiredSteer;
        }
    }
}