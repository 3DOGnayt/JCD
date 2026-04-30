using Components;
using UnityEngine;

namespace Systems.Car.Opponents.AI
{
    public sealed partial class OpponentAISystem
    {
        private float ResolveSpeedInput(
            float speedKmh,
            Vector3 currentPosition,
            Vector3 brakeTargetWorld,
            Vector3 planarForward,
            ref OpponentSplineFollowComponent follow)
        {
            var targetSpeed = Mathf.Max(0f, follow.TargetSpeedKmh);
            var speedScale = ResolveBrakeSpeedScale(currentPosition, brakeTargetWorld, planarForward, ref follow);
            var desiredSpeed = targetSpeed * speedScale;

            var speedDelta = desiredSpeed - speedKmh;
            if (speedDelta >= 0f)
                return Mathf.Clamp(speedDelta / 10f, 0f, 1f);

            return Mathf.Clamp(speedDelta / 10f, -1f, 0f);
        }

        private float ResolveBrakeSpeedScale(
            Vector3 currentPosition,
            Vector3 brakeTargetWorld,
            Vector3 planarForward,
            ref OpponentSplineFollowComponent follow)
        {
            var brakeStrength = Mathf.Clamp01(follow.BrakeStrength);
            if (brakeStrength <= 0f)
                return 1f;

            if (planarForward.sqrMagnitude < 0.001f)
                return 1f;

            var toBrakeTarget = brakeTargetWorld - currentPosition;
            var planarTarget = Vector3.ProjectOnPlane(toBrakeTarget, Vector3.up);
            if (planarTarget.sqrMagnitude < 0.001f)
                return 1f;

            planarTarget.Normalize();
            planarForward.Normalize();

            var signedAngle = Vector3.SignedAngle(planarForward, planarTarget, Vector3.up);
            var absAngle = Mathf.Abs(signedAngle);

            var error = Mathf.Max(0f, follow.SteerErrorDegrees);
            if (absAngle <= error)
                return 1f;

            var maxAngle = Mathf.Max(1f, follow.MaxSteerAngleDeg);
            var severity = Mathf.InverseLerp(error, maxAngle, absAngle);
            var speedScale = 1f - severity * brakeStrength;

            return Mathf.Clamp01(speedScale);
        }
    }
}