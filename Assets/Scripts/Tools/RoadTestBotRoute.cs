#if ROAD_TEST_BOT_ENABLED
using System.Collections.Generic;
using Helpers.Race;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace Tools
{
    [ExecuteAlways]
    public sealed class RoadTestBotRoute : MonoBehaviour
    {
        public enum RouteSource
        {
            UnitySpline,
            EasyRoads
        }

        [Header("Route Source")]
        [SerializeField] private bool _toolEnabled = true;
        [SerializeField] private RouteSource _routeSource = RouteSource.UnitySpline;
        [SerializeField] private MapSplineProvider _mapSplineProvider;
        [SerializeField] private SplineContainer _splineContainer;

        [Header("Bot")]
        [SerializeField] private GameObject _carPrefab;
        [SerializeField] private bool _autoStart = true;
        [SerializeField] private bool _loop = true;
        [SerializeField] private float _spawnHeightOffset = 0.25f;

        [Header("Route")]
        [Range(0f, 100f)]
        [SerializeField] private float _startPercent = 10f;
        [Range(0f, 100f)]
        [SerializeField] private float _endPercent = 15f;
        [Range(-5f, 5f)]
        [SerializeField] private float _lateralOffsetMeters;
        [SerializeField] private bool _reverseRoute;
        [SerializeField] private float _lookAheadMeters = 8f;

        [Header("Driving")]
        [SerializeField] private float _targetSpeedKmh = 35f;
        [SerializeField] private float _motorTorque = 900f;
        [SerializeField] private float _brakeTorque = 2500f;
        [SerializeField] private float _brakeSpeedToleranceKmh = 4f;
        [SerializeField] private float _maxSteerAngle = 35f;
        [SerializeField] private float _steeringSpeed = 120f;

        [Header("Gizmos")]
        [SerializeField] private bool _drawGizmos = true;
        [SerializeField] private int _gizmoSegments = 40;
        [SerializeField] private float _easyRoadSampleStepMeters = 2f;
        [SerializeField] private Color _centerLineColor = new Color(1f, 1f, 1f, 0.45f);
        [SerializeField] private Color _routeLineColor = Color.cyan;
        [SerializeField] private Color _startColor = Color.green;
        [SerializeField] private Color _endColor = Color.red;
        [SerializeField] private float _pointRadius = 0.75f;

        [SerializeField] private List<Vector3> _cachedCenters = new List<Vector3>();
        [SerializeField] private List<float> _cachedDistances = new List<float>();
        [SerializeField] private float _cachedTotalLength;
        [SerializeField] private bool _easyRoadCacheValid;

        private RoadTestBotDriver _activeDriver;
        private GameObject _activeBot;

        public bool ToolEnabled => _toolEnabled;
        public RouteSource Source => _routeSource;
        public SplineContainer SplineContainer => _splineContainer != null ? _splineContainer : (_mapSplineProvider != null ? _mapSplineProvider.Spline : null);
        public GameObject CarPrefab => _carPrefab;
        public bool AutoStart => _autoStart;
        public bool Loop => _loop;
        public float SpawnHeightOffset => _spawnHeightOffset;
        public float StartT => Mathf.Clamp01(_startPercent / 100f);
        public float EndT => Mathf.Clamp01(_endPercent / 100f);
        public float DriveStartT => _reverseRoute ? EndT : StartT;
        public float DriveEndT => _reverseRoute ? StartT : EndT;
        public bool ReverseRoute => _reverseRoute;
        public float LateralOffsetMeters { get => _lateralOffsetMeters; set => _lateralOffsetMeters = value; }
        public float LookAheadMeters => Mathf.Max(0.1f, _lookAheadMeters);
        public float TargetSpeedKmh => Mathf.Max(0f, _targetSpeedKmh);
        public float MotorTorque => Mathf.Max(0f, _motorTorque);
        public float BrakeTorque => Mathf.Max(0f, _brakeTorque);
        public float BrakeSpeedToleranceKmh => Mathf.Max(0f, _brakeSpeedToleranceKmh);
        public float MaxSteerAngle => Mathf.Max(1f, _maxSteerAngle);
        public float SteeringSpeed => Mathf.Max(1f, _steeringSpeed);
        public bool EasyRoadCacheValid => _easyRoadCacheValid;
        public float CachedTotalLength => _cachedTotalLength;
        public int CachedPointCount => _cachedCenters != null ? _cachedCenters.Count : 0;
        public bool DrawGizmosEnabled => _drawGizmos;
        public int GizmoSegments => Mathf.Max(2, _gizmoSegments);
        public float PointRadius => Mathf.Max(0f, _pointRadius);

        private void Reset()
        {
            AutoFindRouteSource();
        }

        private void OnValidate()
        {
            if (_endPercent < _startPercent)
                _endPercent = _startPercent;

            _gizmoSegments = Mathf.Max(2, _gizmoSegments);
            _lookAheadMeters = Mathf.Max(0.1f, _lookAheadMeters);
            _targetSpeedKmh = Mathf.Max(0f, _targetSpeedKmh);
            _motorTorque = Mathf.Max(0f, _motorTorque);
            _brakeTorque = Mathf.Max(0f, _brakeTorque);
            _maxSteerAngle = Mathf.Max(1f, _maxSteerAngle);
            _steeringSpeed = Mathf.Max(1f, _steeringSpeed);
            _lateralOffsetMeters = Mathf.Clamp(_lateralOffsetMeters, -5f, 5f);
            _easyRoadSampleStepMeters = Mathf.Max(0.25f, _easyRoadSampleStepMeters);
        }

        public void UseMapSplineProvider(MapSplineProvider provider)
        {
            _routeSource = RouteSource.UnitySpline;
            _mapSplineProvider = provider;
            _splineContainer = provider != null ? provider.Spline : _splineContainer;
        }

        public void AutoFindRouteSource()
        {
            if (!_toolEnabled)
                return;

            _mapSplineProvider = FindObjectOfType<MapSplineProvider>();
            if (_mapSplineProvider != null)
            {
                _routeSource = RouteSource.UnitySpline;
                _splineContainer = _mapSplineProvider.Spline;
                return;
            }

            _splineContainer = FindObjectOfType<SplineContainer>();
            if (_splineContainer != null)
            {
                _routeSource = RouteSource.UnitySpline;
                return;
            }

            _routeSource = RouteSource.UnitySpline;
            Debug.LogWarning("RoadTestBotRoute: Unity spline route source was not found.");
        }

        public void BuildRouteCache()
        {
            if (!_toolEnabled)
                return;

            _cachedCenters.Clear();
            _cachedDistances.Clear();
            _cachedTotalLength = 0f;
            _easyRoadCacheValid = false;

            if (_routeSource == RouteSource.EasyRoads)
                Debug.LogWarning("RoadTestBotRoute: EasyRoads support was removed. Use UnitySpline route source.");
        }

        public void SpawnBot()
        {
            if (!_toolEnabled)
            {
                Debug.LogWarning("RoadTestBotRoute: tool is disabled.");
                return;
            }

            if (!Application.isPlaying)
            {
                Debug.LogWarning("RoadTestBotRoute: bot can be spawned only in Play Mode.");
                return;
            }

            if (_carPrefab == null)
            {
                Debug.LogWarning("RoadTestBotRoute: car prefab is not assigned.");
                return;
            }

            if (!HasRoute())
            {
                Debug.LogWarning("RoadTestBotRoute: route source is not assigned.");
                return;
            }

            StopAndDestroyBot();

            _activeBot = Instantiate(_carPrefab);
            _activeBot.name = _carPrefab.name + " Road Test Bot";
            _activeDriver = _activeBot.GetComponent<RoadTestBotDriver>();
            if (_activeDriver == null)
                _activeDriver = _activeBot.AddComponent<RoadTestBotDriver>();

            _activeDriver.Initialize(this);
        }

        public void StartBot()
        {
            if (!_toolEnabled)
                return;

            if (_activeDriver == null)
                SpawnBot();

            _activeDriver?.StartRun();
        }

        public void RestartBot()
        {
            if (!_toolEnabled)
                return;

            if (_activeDriver == null)
                SpawnBot();

            _activeDriver?.RestartRun();
        }

        public void StopBot()
        {
            _activeDriver?.StopRun();
        }

        public void SetToolEnabled(bool enabled)
        {
            _toolEnabled = enabled;

            if (!_toolEnabled)
                StopAndDestroyBot();
        }

        public void StopAndDestroyBot()
        {
            if (_activeBot == null)
                return;

            if (Application.isPlaying)
                Destroy(_activeBot);
            else
                DestroyImmediate(_activeBot);

            _activeBot = null;
            _activeDriver = null;
        }

        public Vector3 GetWorldPoint(float t)
        {
            if (_routeSource == RouteSource.EasyRoads)
                return transform.position;

            var container = SplineContainer;
            if (container == null || container.Spline == null)
                return transform.position;

            return container.transform.TransformPoint((Vector3)container.Spline.EvaluatePosition(Mathf.Clamp01(t)));
        }

        public float GetNearestT(Vector3 worldPosition, float currentT)
        {
            if (_routeSource == RouteSource.EasyRoads)
                return currentT;

            var container = SplineContainer;
            if (container == null || container.Spline == null)
                return currentT;

            var localPosition = container.transform.InverseTransformPoint(worldPosition);
            SplineUtility.GetNearestPoint(container.Spline, (float3)localPosition, out _, out var nearestT);
            return nearestT;
        }

        public Vector3 GetWorldPointAhead(float t, float distanceMeters)
        {
            if (_routeSource == RouteSource.EasyRoads)
                return transform.position;

            var container = SplineContainer;
            if (container == null || container.Spline == null)
                return transform.position;

            var local = container.Spline.GetPointAtLinearDistance(Mathf.Clamp01(t), _reverseRoute ? -Mathf.Max(0f, distanceMeters) : Mathf.Max(0f, distanceMeters), out _);
            return container.transform.TransformPoint((Vector3)local);
        }

        public Quaternion GetWorldRotation(float t, Vector3 fallbackForward)
        {
            if (_routeSource == RouteSource.EasyRoads)
                return Quaternion.LookRotation(fallbackForward, Vector3.up);

            var container = SplineContainer;
            if (container == null || container.Spline == null)
                return Quaternion.LookRotation(fallbackForward, Vector3.up);

            if (!container.Evaluate(container.Spline, Mathf.Clamp01(t), out _, out var tangent, out var up))
                return Quaternion.LookRotation(fallbackForward, Vector3.up);

            var splineForward = ((Vector3)tangent).sqrMagnitude > 0.001f ? ((Vector3)tangent).normalized : fallbackForward;
            if (_reverseRoute)
                splineForward = -splineForward;

            var upVector = ((Vector3)up).sqrMagnitude > 0.001f ? ((Vector3)up).normalized : Vector3.up;

            return Quaternion.LookRotation(splineForward, upVector);
        }

        public Vector3 GetWorldForward(float t, Vector3 fallbackForward)
        {
            return GetWorldRotation(t, fallbackForward) * Vector3.forward;
        }

        public bool HasPassedDriveEnd(Vector3 worldPosition, float progressT)
        {
            const float ProgressTolerance = 0.0005f;
            const float EndPlaneToleranceMeters = 0.5f;

            if (_reverseRoute)
            {
                if (progressT <= DriveEndT + ProgressTolerance)
                    return true;
            }
            else
            {
                if (progressT >= DriveEndT - ProgressTolerance)
                    return true;
            }

            var endPosition = ApplyLateralOffset(GetWorldPoint(DriveEndT), DriveEndT);
            var endForward = Vector3.ProjectOnPlane(GetWorldForward(DriveEndT, transform.forward), Vector3.up);
            var toCar = Vector3.ProjectOnPlane(worldPosition - endPosition, Vector3.up);

            if (endForward.sqrMagnitude < 0.001f || toCar.sqrMagnitude < EndPlaneToleranceMeters * EndPlaneToleranceMeters)
                return false;

            return Vector3.Dot(toCar.normalized, endForward.normalized) > 0f;
        }

        public Vector3 ApplyLateralOffset(Vector3 centerPoint, float t)
        {
            return centerPoint + GetRightVector(t) * _lateralOffsetMeters;
        }

        private Vector3 GetRightVector(float t)
        {
            if (_routeSource == RouteSource.EasyRoads)
                return transform.right;

            var container = SplineContainer;
            if (container == null || container.Spline == null)
                return transform.right;

            if (!container.Evaluate(container.Spline, Mathf.Clamp01(t), out _, out var tangent, out var up))
                return transform.right;

            var splineForward = (Vector3)tangent;
            if (_reverseRoute)
                splineForward = -splineForward;

            var upVector = (Vector3)up;
            if (splineForward.sqrMagnitude < 0.001f)
                splineForward = transform.forward;
            if (upVector.sqrMagnitude < 0.001f)
                upVector = Vector3.up;

            return Vector3.Cross(upVector.normalized, splineForward.normalized).normalized;
        }

        private void OnDrawGizmos()
        {
            if (!_drawGizmos)
                return;

            if (!_toolEnabled)
                return;

            if (!HasRoute())
                return;

            DrawRouteLine(false);
            DrawRouteLine(true);

            var start = ApplyLateralOffset(GetWorldPoint(DriveStartT), DriveStartT);
            var end = ApplyLateralOffset(GetWorldPoint(DriveEndT), DriveEndT);

            Gizmos.color = _startColor;
            Gizmos.DrawSphere(start, _pointRadius);

            Gizmos.color = _endColor;
            Gizmos.DrawSphere(end, _pointRadius);
        }

        private void DrawRouteLine(bool withOffset)
        {
            Gizmos.color = withOffset ? _routeLineColor : _centerLineColor;

            var previous = withOffset ? ApplyLateralOffset(GetWorldPoint(StartT), StartT) : GetWorldPoint(StartT);
            for (var i = 1; i <= _gizmoSegments; i++)
            {
                var t = Mathf.Lerp(StartT, EndT, i / (float)_gizmoSegments);
                var current = withOffset ? ApplyLateralOffset(GetWorldPoint(t), t) : GetWorldPoint(t);
                Gizmos.DrawLine(previous, current);
                previous = current;
            }
        }

        public bool HasRoute()
        {
            if (_routeSource == RouteSource.EasyRoads)
                return false;

            return SplineContainer != null && SplineContainer.Spline != null;
        }
    }
}
#endif
