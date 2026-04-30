using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace Systems.Car.Opponents.Rail
{
    public sealed partial class OpponentRailSystem
    {
        private static Quaternion ResolveRailRotation(
            SplineContainer container,
            Spline spline,
            float t,
            Vector3 fallbackForward,
            Vector3 fallbackUp)
        {
            if (container == null || spline == null || spline.Count == 0)
                return Quaternion.LookRotation(fallbackForward, fallbackUp.sqrMagnitude > 0.001f ? fallbackUp : Vector3.up);

            var curveIndex = SplineUtility.SplineToCurveT(spline, t, out var curveT);
            var nextIndex = spline.Closed ? (curveIndex + 1) % spline.Count : Mathf.Min(curveIndex + 1, spline.Count - 1);

            var rotA = ToUnityQuaternion(spline[curveIndex].Rotation);
            var rotB = ToUnityQuaternion(spline[nextIndex].Rotation);
            var localRotation = Quaternion.Slerp(rotA, rotB, curveT);

            if (localRotation == Quaternion.identity && fallbackForward.sqrMagnitude > 0.001f)
                return Quaternion.LookRotation(fallbackForward, fallbackUp.sqrMagnitude > 0.001f ? fallbackUp : Vector3.up);

            return container.transform.rotation * localRotation;
        }

        private static Quaternion ToUnityQuaternion(quaternion value)
        {
            return new Quaternion(value.value.x, value.value.y, value.value.z, value.value.w);
        }
    }
}