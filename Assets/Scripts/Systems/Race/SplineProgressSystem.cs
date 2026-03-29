using Components;
using Configs.Impl;
using Scellecs.Morpeh;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using Zenject;

namespace Systems.Race
{
    public sealed class SplineProgressSystem : ISystem
    {
        [Inject] public World World { get; set; }
        [Inject] private GameSelectionParameters _gameSelectionParameters;

        private Filter _trackEntities;
        private Filter _players;
        private Filter _opponents;

        private Stash<TransformComponent> _transformStash;
        private Stash<SplineProgressComponent> _progressStash;
        private Stash<SplineDeltaComponent> _deltaStash;

        private SplineContainer _splineContainer;
        private Spline _spline;
        private float _splineLength;

        public void OnAwake()
        {
            _trackEntities = World.Filter
                .With<TransformComponent>()
                .With<SplineProgressComponent>()
                .Build();

            _players = World.Filter
                .With<PlayerTagComponent>()
                .With<SplineProgressComponent>()
                .With<SplineDeltaComponent>()
                .Build();

            _opponents = World.Filter
                .With<OpponentTagComponent>()
                .With<SplineProgressComponent>()
                .Build();

            _transformStash = World.GetStash<TransformComponent>();
            _progressStash = World.GetStash<SplineProgressComponent>();
            _deltaStash = World.GetStash<SplineDeltaComponent>();
        }

        public void OnUpdate(float deltaTime)
        {
            if (!TryResolveSpline())
                return;

            foreach (var entity in _trackEntities)
            {
                var tr = _transformStash.Get(entity).Value;
                if (tr == null)
                    continue;

                var localPos = _splineContainer.transform.InverseTransformPoint(tr.position);
                SplineUtility.GetNearestPoint(_spline, (float3)localPos, out _, out var t);

                ref var progress = ref _progressStash.Get(entity);
                UpdateProgress(ref progress, t);
            }

            UpdateDelta();
        }

        private bool TryResolveSpline()
        {
            var runtimeSpline = _gameSelectionParameters != null ? _gameSelectionParameters.RuntimeSpline : null;
            if (runtimeSpline == null)
                return false;

            if (_splineContainer == runtimeSpline && _spline != null && _splineLength > 0f)
                return true;

            _splineContainer = runtimeSpline;
            _spline = _splineContainer.Spline;
            _splineLength = _spline != null
                ? SplineUtility.CalculateLength(_spline, _splineContainer.transform.localToWorldMatrix)
                : 0f;

            return _spline != null && _splineLength > 0f;
        }

        private void UpdateDelta()
        {
            if (_players.IsEmpty())
                return;

            if (_opponents.IsEmpty())
            {
                foreach (var player in _players)
                    _deltaStash.Get(player).Value = float.NaN;
                return;
            }

            var playerEntity = _players.First();
            var opponentEntity = _opponents.First();

            var playerProgress = _progressStash.Get(playerEntity);
            var opponentProgress = _progressStash.Get(opponentEntity);

            var playerDistance = playerProgress.Distance;
            var opponentDistance = opponentProgress.Distance;

            _deltaStash.Get(playerEntity).Value = playerDistance - opponentDistance;
        }

        private void UpdateProgress(ref SplineProgressComponent progress, float t)
        {
            if (progress.HasPrev && progress.PrevT > 0.8f && t < 0.2f)
                progress.Laps++;

            progress.PrevT = t;
            progress.HasPrev = true;
            progress.Distance = (progress.Laps + t) * _splineLength;
        }

        public void Dispose() { }
    }
}