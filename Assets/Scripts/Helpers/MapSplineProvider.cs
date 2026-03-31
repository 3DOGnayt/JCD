using UnityEngine;
using UnityEngine.Splines;

namespace Helpers
{
    public sealed class MapSplineProvider : MonoBehaviour
    {
        [SerializeField] private SplineContainer _spline;
        [SerializeField] private SplineContainer _innerSpline;
        [SerializeField] private SplineContainer _outerSpline;

        public SplineContainer Spline => _spline;
        public SplineContainer InnerSpline => _innerSpline;
        public SplineContainer OuterSpline => _outerSpline;
    }
}