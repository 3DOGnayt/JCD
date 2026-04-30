using Components;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace Systems.Car.Opponents.Rail
{
    public sealed partial class OpponentRailSystem
    {
        private bool TryResolveSpline(int opponentIndex, out SplineContainer container, out Spline spline)
        {
            container = null;
            spline = null;

            var opponentSplines = _gameSelectionParameters != null ? _gameSelectionParameters.RuntimeOpponentSplines : null;
            if (opponentSplines != null)
            {
                for (var i = 0; i < opponentSplines.Length; i++)
                {
                    if (opponentSplines[i].OpponentIndex != opponentIndex)
                        continue;

                    container = opponentSplines[i].Spline;
                    if (container != null)
                        break;
                }
            }

            if (container == null)
                container = _gameSelectionParameters != null ? _gameSelectionParameters.RuntimeSpline : null;

            if (container == null)
                return false;

            spline = container.Spline;
            return spline != null;
        }

        private static void InitializeRailProgress(
            SplineContainer container,
            Spline spline,
            Vector3 worldPosition,
            ref OpponentSplineFollowComponent follow)
        {
            var localPos = container.transform.InverseTransformPoint(worldPosition);
            SplineUtility.GetNearestPoint(spline, (float3)localPos, out _, out var t);
            follow.ProgressT = t;
            follow.RailInitialized = true;
        }

        private static void AdvanceProgress(
            Spline spline,
            float speedKmh,
            float deltaTime,
            ref OpponentSplineFollowComponent follow)
        {
            var speedMps = Mathf.Max(0f, speedKmh) / 3.6f;
            var distance = speedMps * deltaTime;
            if (distance <= 0f)
                return;

            var length = spline.GetLength();
            if (length <= 0f)
                return;

            if (spline.Closed)
            {
                var currentDistance = follow.ProgressT * length;
                var newDistance = Mathf.Repeat(currentDistance + distance, length);
                follow.ProgressT = newDistance / length;
                return;
            }

            _ = spline.GetPointAtLinearDistance(follow.ProgressT, distance, out var nextT);
            follow.ProgressT = nextT;
        }
    }
}