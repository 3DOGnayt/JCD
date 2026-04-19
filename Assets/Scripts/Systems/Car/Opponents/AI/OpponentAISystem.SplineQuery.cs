using Components;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace Systems.Car.Opponents.AI
{
    public sealed partial class OpponentAISystem
    {
        private bool TryGetSplineTargets(
            Transform tr,
            float trajectoryBlend,
            ref OpponentSplineFollowComponent follow,
            out Vector3 targetWorld,
            out Vector3 brakeTargetWorld,
            out Vector3 planarForward)
        {
            _ = trajectoryBlend;
            targetWorld = default;
            brakeTargetWorld = default;
            planarForward = Vector3.zero;

            if (tr == null)
                return false;

            if (!TryResolveProgressT(tr.position, ref follow))
                return false;

            var lookAhead = Mathf.Max(0.1f, follow.LookAheadMeters);
            var brakeLookAhead = Mathf.Max(0.1f, follow.BrakeLookAheadMeters > 0f ? follow.BrakeLookAheadMeters : lookAhead);

            if (_spline != null && _splineContainer != null)
            {
                var targetLocal = _spline.GetPointAtLinearDistance(follow.ProgressT, lookAhead, out _);
                var brakeLocal = _spline.GetPointAtLinearDistance(follow.ProgressT, brakeLookAhead, out _);
                targetWorld = _splineContainer.transform.TransformPoint((Vector3)targetLocal);
                brakeTargetWorld = _splineContainer.transform.TransformPoint((Vector3)brakeLocal);
            }
            else
            {
                return false;
            }
            planarForward = Vector3.ProjectOnPlane(tr.forward, Vector3.up);

            return true;
        }

        private bool TryResolveProgressT(Vector3 position, ref OpponentSplineFollowComponent follow)
        {
            if (_splineContainer != null && _spline != null)
            {
                var localPos = _splineContainer.transform.InverseTransformPoint(position);
                SplineUtility.GetNearestPoint(_spline, (float3)localPos, out _, out var t);
                follow.ProgressT = t;
                return true;
            }

            return false;
        }

        private float ResolveTrajectoryBlend(float speedKmh, ref OpponentSplineFollowComponent follow)
        {
            var maxSpeed = Mathf.Max(1f, follow.TargetSpeedKmh);
            return Mathf.Clamp01(speedKmh / maxSpeed);
        }
    }
}