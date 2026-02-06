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
        [Inject] private GameSelectionParameters _gameSelectionParameters;
        [Inject] private DiContainer _container;
        [Inject] private ILoadingService _loadingService;

        private Transform _mapRoot;
        private IDisposable _startRaceDisposable;

        public void OnAwake()
        {
            _mapRoot = new GameObject("Map").transform;
            _startRaceDisposable = _loadingService.StartRaceStream.Subscribe(_ => OnStartRace());
        }

        private void OnStartRace()
        {
            var prefab = _gameSelectionParameters != null ? _gameSelectionParameters.SelectedMapPrefab : null;
            if (prefab == null)
                return;

            _container.InstantiatePrefab(prefab, Vector3.zero, Quaternion.identity, _mapRoot);
        }

        public void OnUpdate(float deltaTime) { }

        public void Dispose()
        {
            _startRaceDisposable?.Dispose();
        }
    }
}
