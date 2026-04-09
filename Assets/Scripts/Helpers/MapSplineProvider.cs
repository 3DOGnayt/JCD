using UnityEngine;
using UnityEngine.Splines;

namespace Helpers
{
    public sealed class MapSplineProvider : MonoBehaviour
    {
        [System.Serializable]
        public struct OpponentSplineEntry
        {
            public int OpponentIndex;
            public SplineContainer Spline;
        }

        [SerializeField] private SplineContainer _spline;
        [SerializeField] private SplineContainer _innerSpline;
        [SerializeField] private SplineContainer _outerSpline;
        [SerializeField] private OpponentSplineEntry[] _opponentSplines;

        public SplineContainer Spline => _spline;
        public SplineContainer InnerSpline => _innerSpline;
        public SplineContainer OuterSpline => _outerSpline;
        public OpponentSplineEntry[] OpponentSplines => _opponentSplines;
    }
}