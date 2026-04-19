using Components;
using UnityEngine;
using UnityEngine.Splines;

namespace Systems.Car.Opponents.Rail
{
    public sealed partial class OpponentRailSystem
    {
        private static float ResolveBrakeSpeedScale(Spline spline, float progressT, ref OpponentSplineFollowComponent follow)
        {
            var brakeStrength = Mathf.Clamp01(follow.BrakeStrength);
            if (brakeStrength <= 0f || spline == null)
                return 1f;

            var lookAhead = Mathf.Max(0.1f, follow.BrakeLookAheadMeters > 0f ? follow.BrakeLookAheadMeters : follow.LookAheadMeters);
            if (lookAhead <= 0f)
                return 1f;

            var tangentNow = (Vector3)SplineUtility.EvaluateTangent(spline, progressT);
            if (tangentNow.sqrMagnitude < 0.001f)
                return 1f;

            spline.GetPointAtLinearDistance(progressT, lookAhead, out var brakeT);
            var tangentAhead = (Vector3)SplineUtility.EvaluateTangent(spline, brakeT);
            if (tangentAhead.sqrMagnitude < 0.001f)
                return 1f;

            tangentNow.Normalize();
            tangentAhead.Normalize();

            var absAngle = Vector3.Angle(tangentNow, tangentAhead);
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