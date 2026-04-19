using Components;
using Configs.Impl;
using Scellecs.Morpeh;
using Services;
using UnityEngine.Splines;
using Zenject;

namespace Systems.Car.Opponents.AI
{
    public sealed partial class OpponentAISystem : ISystem
    {
        [Inject] public World World { get; set; }
        [Inject] private GameSelectionParameters _gameSelectionParameters;
        [Inject] private IUnitRaceTimerService _unitRaceTimerService;

        private Filter _opponents;
        private Stash<TransformComponent> _transformStash;
        private Stash<SpeedComponent> _speedStash;
        private Stash<VerticalInputComponent> _verticalStash;
        private Stash<HorizontalInputComponent> _horizontalStash;
        private Stash<HandbrakeInputComponent> _handbrakeStash;
        private Stash<OpponentSplineFollowComponent> _followStash;

        private SplineContainer _splineContainer;
        private Spline _spline;
        private SplineContainer _splineContainerInner;
        private Spline _splineInner;
        private SplineContainer _splineContainerOuter;
        private Spline _splineOuter;
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
            if (_needsSpline || (_splineContainer == null && _splineContainerInner == null && _splineContainerOuter == null))
            {
                if (!TryResolveSpline())
                    return;

                _needsSpline = false;
            }

            foreach (var opponent in _opponents)
            {
                if (_unitRaceTimerService != null && _unitRaceTimerService.IsFinished(opponent))
                {
                    _verticalStash.Get(opponent).Value = 0f;
                    _horizontalStash.Get(opponent).Value = 0f;
                    _handbrakeStash.Get(opponent).Value = true;
                    continue;
                }

                var tr = _transformStash.Get(opponent).Value;
                if (tr == null)
                    continue;

                ref var follow = ref _followStash.Get(opponent);

                var speedKmh = _speedStash.Get(opponent).Value;
                var trajectoryBlend = ResolveTrajectoryBlend(speedKmh, ref follow);
                if (!TryGetSplineTargets(tr, trajectoryBlend, ref follow, out var targetWorld, out var brakeTargetWorld, out var planarForward))
                    continue;

                var desiredSteer = ResolveSteering(tr.position, targetWorld, planarForward, ref follow, out _);
                desiredSteer = ApplySteeringDelay(desiredSteer, ref follow, deltaTime);
                _horizontalStash.Get(opponent).Value = desiredSteer;

                var verticalInput = ResolveSpeedInput(speedKmh, tr.position, brakeTargetWorld, planarForward, ref follow);
                _verticalStash.Get(opponent).Value = verticalInput;
                _handbrakeStash.Get(opponent).Value = false;
            }
        }

        private bool TryResolveSpline()
        {
            if (_splineContainer != null)
                return true;

            var runtimeSpline = _gameSelectionParameters != null ? _gameSelectionParameters.RuntimeSpline : null;
            var runtimeSplineInner = _gameSelectionParameters != null ? _gameSelectionParameters.RuntimeSplineInner : null;
            var runtimeSplineOuter = _gameSelectionParameters != null ? _gameSelectionParameters.RuntimeSplineOuter : null;

            if (runtimeSpline == null)
                return false;

            _splineContainer = runtimeSpline;
            _spline = _splineContainer != null ? _splineContainer.Spline : null;
            _splineContainerInner = runtimeSplineInner;
            _splineInner = _splineContainerInner != null ? _splineContainerInner.Spline : null;
            _splineContainerOuter = runtimeSplineOuter;
            _splineOuter = _splineContainerOuter != null ? _splineContainerOuter.Spline : null;

            return _spline != null;
        }

        public void Dispose() { }
    }
}