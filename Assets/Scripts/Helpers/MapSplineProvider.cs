using UnityEngine;
using UnityEngine.Splines;

namespace Helpers
{
    public sealed class MapSplineProvider : MonoBehaviour
    {
        [SerializeField] private SplineContainer _spline;

        public SplineContainer Spline => _spline;
    }
}