using Configs.Impl;
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

        private ILoadingService _loadingService;
        private IGameSessionService _gameSessionService;
        private MapSelectionParameters _mapSelectionParameters;

        private Transform _mapRoot;
        private IDisposable _startRaceDisposable;
        
        [Inject]
        public void Construct(
            ILoadingService loadingService,
            IGameSessionService gameSessionService,
            MapSelectionParameters mapSelectionParameters
        )
        {
            _loadingService = loadingService;
            _gameSessionService = gameSessionService;
            _mapSelectionParameters = mapSelectionParameters;
        }

        public void OnAwake()
        {
            _mapRoot = new GameObject("Map").transform;
            _startRaceDisposable = _loadingService.StartRaceStream.Subscribe(_ => OnStartRace());
        }

        private void OnStartRace()
        {
            var prefab = _mapSelectionParameters != null ? _mapSelectionParameters.SelectedMapPrefab : null;
            if (prefab == null)
                return;

            var instance = _container.InstantiatePrefab(prefab, Vector3.zero, Quaternion.identity, _mapRoot);
            _gameSessionService?.RegisterRuntimeRoot(instance);
        }

        public void OnUpdate(float deltaTime) { }

        public void Dispose()
        {
            _startRaceDisposable?.Dispose();
        }
    }
}