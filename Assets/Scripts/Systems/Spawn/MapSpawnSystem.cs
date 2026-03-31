using Configs.Impl;
using Data.Enums;
using Helpers;
using Scellecs.Morpeh;
using Services;
using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace Systems.Spawn
{
    public sealed class MapSpawnSystem : ISystem
    {
        [Inject] public World World { get; set; }
        [Inject] private DiContainer _container;

        private IEventService _eventService;
        private IGameSessionService _gameSessionService;
        private MapSelectionParameters _mapSelectionParameters;

        private Transform _mapRoot;
        private IDisposable _spawnDisposable;
        private const float SpawnProgressThreshold = 0.2f;
        private bool _hasSpawnedThisLoad;
        
        [Inject]
        public void Construct(
            IEventService eventService,
            IGameSessionService gameSessionService,
            MapSelectionParameters mapSelectionParameters
        )
        {
            _eventService = eventService;
            _gameSessionService = gameSessionService;
            _mapSelectionParameters = mapSelectionParameters;
        }

        public void OnAwake()
        {
            _mapRoot = new GameObject("Map").transform;
            if (_eventService != null)
            {
                _spawnDisposable = _eventService.LoadingProgress.Subscribe(OnLoadingProgress);
            }
        }

        private void OnLoadingProgress(float progress)
        {
            if (progress <= 0f)
                _hasSpawnedThisLoad = false;

            if (_gameSessionService != null && _gameSessionService.Target != EGameSessionTarget.Game)
                return;

            if (_hasSpawnedThisLoad || progress < SpawnProgressThreshold)
                return;

            _hasSpawnedThisLoad = true;
            OnStartRace();
        }

        private void OnStartRace()
        {
            var prefab = _mapSelectionParameters != null ? _mapSelectionParameters.SelectedMapPrefab : null;
            if (prefab == null)
                return;

            var instance = _container.InstantiatePrefab(prefab, Vector3.zero, Quaternion.identity, _mapRoot);
            var splineProvider = instance.GetComponent<MapSplineProvider>();
            if (splineProvider == null)
                splineProvider = instance.GetComponentInChildren<MapSplineProvider>();

            var innerSpline = splineProvider != null ? splineProvider.InnerSpline : null;
            var outerSpline = splineProvider != null ? splineProvider.OuterSpline : null;
            var runtimeSpline = splineProvider != null ? splineProvider.Spline : null;
            if (runtimeSpline == null)
                runtimeSpline = innerSpline != null ? innerSpline : outerSpline;

            _mapSelectionParameters?.SetRuntimeSpline(runtimeSpline);
            _mapSelectionParameters?.SetRuntimeSplines(innerSpline, outerSpline);

            _gameSessionService?.RegisterRuntimeRoot(instance);
        }

        public void OnUpdate(float deltaTime) { }

        public void Dispose()
        {
            _spawnDisposable?.Dispose();
        }
    }
}