using Cameras;
using Components;
using Scellecs.Morpeh;
using Services;
using Zenject;

namespace Systems.Spawn
{
    public sealed class MinimapSpawnSystem : ISystem
    {
        [Inject] public World World { get; set; }
        [Inject] private MinimapCameraHolder _minimapCameraPrefab;
        [Inject] private DiContainer _container;
        [Inject] private IEventService _eventService;

        private bool _hasSpawned;

        public void OnAwake()
        {
            TrySpawn();
        }

        public void OnUpdate(float deltaTime)
        {
            if (_hasSpawned)
                return;

            TrySpawn();
        }

        private void TrySpawn()
        {
            if (_hasSpawned || _minimapCameraPrefab == null || _container == null)
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

            _eventService?.PublishMinimapSpawned(instance);
            _hasSpawned = true;
        }

        public void Dispose() { }
    }
}