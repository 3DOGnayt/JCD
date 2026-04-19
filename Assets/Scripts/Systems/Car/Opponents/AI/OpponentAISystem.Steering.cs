using Components;
using UnityEngine;

namespace Systems.Car
{
    public sealed partial class OpponentAISystem
    {
        private float ResolveSteering(
            Vector3 currentPosition,
            Vector3 targetWorld,
            Vector3 planarForward,
            ref OpponentSplineFollowComponent follow,
            out float absAngleDeg)
        {
            absAngleDeg = 0f;

            var toTarget = targetWorld - currentPosition;
            var planarTarget = Vector3.ProjectOnPlane(toTarget, Vector3.up);

            if (planarTarget.sqrMagnitude < 0.001f || planarForward.sqrMagnitude < 0.001f)
                return 0f;

            planarTarget.Normalize();
            planarForward.Normalize();

            var signedAngle = Vector3.SignedAngle(planarForward, planarTarget, Vector3.up);
            absAngleDeg = Mathf.Abs(signedAngle);

            var error = Mathf.Max(0f, follow.SteerErrorDegrees);
            var effectiveAngle = absAngleDeg - error;
            if (effectiveAngle <= 0f)
                return 0f;

            var maxAngle = Mathf.Max(1f, follow.MaxSteerAngleDeg);
            var steerValue = Mathf.Clamp(effectiveAngle / maxAngle, 0f, 1f);

            return Mathf.Sign(signedAngle) * steerValue;
        }
    }
}