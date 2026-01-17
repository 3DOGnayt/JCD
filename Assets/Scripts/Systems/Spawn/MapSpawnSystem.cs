using Configs.Impl;
using Scellecs.Morpeh;
using Signals;
using UnityEngine;
using Zenject;

namespace Systems.Spawn
{
    public sealed class MapSpawnSystem : ISystem
    {
        [Inject] public World World { get; set; }
        [Inject] private GameSelectionParameters _gameSelectionParameters;
        [Inject] private DiContainer _container;
        [Inject] private SignalBus _signalBus;

        private Transform _mapRoot;
        private GameObject _currentMap;

        public void OnAwake()
        {
            _mapRoot = new GameObject("Map").transform;
            _signalBus.Subscribe<StartRaceSignal>(OnStartRace);
        }

        private void OnStartRace()
        {
            var prefab = _gameSelectionParameters != null ? _gameSelectionParameters.SelectedMapPrefab : null;
            if (prefab == null)
                return;

            if (_currentMap != null)
                Object.Destroy(_currentMap);

            var instance = _container.InstantiatePrefab(prefab, Vector3.zero, Quaternion.identity, _mapRoot);
            _currentMap = instance;
        }

        public void OnUpdate(float deltaTime) { }

        public void Dispose()
        {
            if (_signalBus != null)
                _signalBus.Unsubscribe<StartRaceSignal>(OnStartRace);
        }
    }
}
