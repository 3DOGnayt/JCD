using Cameras;
using Components;
using Data.Enums;
using Scellecs.Morpeh;
using Services;
using System;
using UniRx;
using Zenject;

namespace Systems.Spawn
{
    public sealed class MinimapSpawnSystem : ISystem
    {
        [Inject] public World World { get; set; }
        [Inject] private MinimapCameraHolder _minimapCameraPrefab;
        [Inject] private DiContainer _container;
        [Inject] private IEventService _eventService;
        [Inject] private IGameSessionService _gameSessionService;

        private IDisposable _spawnDisposable;
        
        private bool _hasSpawned;
        private const float SpawnProgressThreshold = 0.2f;

        public void OnAwake()
        {
            if (_eventService != null)
                _spawnDisposable = _eventService.LoadingProgress.Subscribe(OnLoadingProgress);
        }

        public void OnUpdate(float deltaTime) { }

        private void OnLoadingProgress(float progress)
        {
            if (progress <= 0f)
                _hasSpawned = false;

            if (_gameSessionService != null && _gameSessionService.Target != EGameSessionTarget.Game)
                return;

            if (_hasSpawned || progress < SpawnProgressThreshold)
                return;

            TrySpawn();
        }

        private void TrySpawn()
        {
            if (_hasSpawned || _minimapCameraPrefab == null || _container == null)
                return;
            if (_gameSessionService != null && _gameSessionService.Target != EGameSessionTarget.Game)
                return;

            var prefabTransform = _minimapCameraPrefab.transform;
            var instance = _container.InstantiatePrefabForComponent<MinimapCameraHolder>(
                _minimapCameraPrefab.gameObject,
                prefabTransform.position,
                prefabTransform.rotation,
                null);

            if (instance == null)
                return;

            var cameraEntity = World.CreateEntity();
            var followPivot = instance.transform;

            cameraEntity.SetComponent(new MiniMapTagComponent());
            cameraEntity.SetComponent(new TransformComponent { Value = followPivot});

            _gameSessionService?.RegisterRuntimeEntity(cameraEntity);
            _gameSessionService?.RegisterRuntimeRoot(instance.gameObject);
            _eventService?.PublishMinimapSpawned(instance);
            _hasSpawned = true;
        }

        public void Dispose()
        {
            _spawnDisposable?.Dispose();
        }
    }
}