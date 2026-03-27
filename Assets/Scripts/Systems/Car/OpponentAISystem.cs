using Components;
using Configs.Impl;
using Scellecs.Morpeh;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using Zenject;

namespace Systems.Car
{
    public sealed class OpponentAISystem : ISystem
    {
        [Inject] public World World { get; set; }
        [Inject] private GameSelectionParameters _gameSelectionParameters;

        private Filter _opponents;
        private Stash<TransformComponent> _transformStash;
        private Stash<SpeedComponent> _speedStash;
        private Stash<VerticalInputComponent> _verticalStash;
        private Stash<HorizontalInputComponent> _horizontalStash;
        private Stash<HandbrakeInputComponent> _handbrakeStash;
        private Stash<OpponentSplineFollowComponent> _followStash;

        private SplineContainer _splineContainer;
        private Spline _spline;
        private bool _needsSpline = true;

        public void OnAwake()
        {
            _opponents = World.Filter
                .With<OpponentTagComponent>()
                .With<TransformComponent>()
                .With<SpeedComponent>()
                .With<VerticalInputComponent>()
                .With<HorizontalInputComponent>()
                .With<HandbrakeInputComponent>()
                .With<OpponentSplineFollowComponent>()
                .Build();

            _transformStash = World.GetStash<TransformComponent>();
            _speedStash = World.GetStash<SpeedComponent>();
            _verticalStash = World.GetStash<VerticalInputComponent>();
            _horizontalStash = World.GetStash<HorizontalInputComponent>();
            _handbrakeStash = World.GetStash<HandbrakeInputComponent>();
            _followStash = World.GetStash<OpponentSplineFollowComponent>();
        }

        public void OnUpdate(float deltaTime)
        {
            if (_needsSpline || _splineContainer == null)
            {
                if (!TryResolveSpline())
                    return;

                _needsSpline = false;
            }

            foreach (var opponent in _opponents)
            {
                var tr = _transformStash.Get(opponent).Value;
                if (tr == null)
                    continue;

                ref var follow = ref _followStash.Get(opponent);

                var localPos = _splineContainer.transform.InverseTransformPoint(tr.position);
                SplineUtility.GetNearestPoint(_spline, (float3)localPos, out _, out var t);
                follow.ProgressT = t;

                var targetLocal = _spline.GetPointAtLinearDistance(t, follow.LookAheadMeters, out _);
                var targetWorld = _splineContainer.transform.TransformPoint((Vector3)targetLocal);

                var toTarget = targetWorld - tr.position;
                var planarTarget = Vector3.ProjectOnPlane(toTarget, Vector3.up);
                var planarForward = Vector3.ProjectOnPlane(tr.forward, Vector3.up);

                if (planarTarget.sqrMagnitude < 0.001f || planarForward.sqrMagnitude < 0.001f)
                {
                    _horizontalStash.Get(opponent).Value = 0f;
                }
                else
                {
                    planarTarget.Normalize();
                    planarForward.Normalize();

                    var angle = Vector3.SignedAngle(planarForward, planarTarget, Vector3.up);
                    var maxAngle = Mathf.Max(1f, follow.MaxSteerAngleDeg);

                    _horizontalStash.Get(opponent).Value = Mathf.Clamp(angle / maxAngle, -1f, 1f);
                }

                var speedKmh = _speedStash.Get(opponent).Value;
                var targetSpeed = Mathf.Max(0f, follow.TargetSpeedKmh);
                var speedDelta = targetSpeed - speedKmh;

                _verticalStash.Get(opponent).Value = Mathf.Clamp(speedDelta / 10f, 0f, 1f);
                _handbrakeStash.Get(opponent).Value = false;
            }
        }

        private bool TryResolveSpline()
        {
            if (_splineContainer != null)
                return true;

            var runtimeSpline = _gameSelectionParameters != null ? _gameSelectionParameters.RuntimeSpline : null;
            if (runtimeSpline == null)
                return false;

            _splineContainer = runtimeSpline;
            _spline = _splineContainer.Spline;
            return _spline != null;
        }

        public void Dispose() { }
    }
}