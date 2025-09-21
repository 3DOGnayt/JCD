using Scellecs.Morpeh;
using Signals;
using Tools;
using UnityEngine;
using Zenject;

namespace Systems
{
    public sealed class CameraSpawnSystem : ISystem
    {
        [Inject] public World World { get; set; }
        [Inject] private FollowingCamera _cameraPrefab;
        [Inject] private DiContainer _container;
        [Inject] private SignalBus _signalBus;

        private Transform _cameraGroup;

        public void OnAwake()
        {
            SetSpawnRoot();
            SpawnCamera();
        }

        private void SetSpawnRoot() => _cameraGroup = new GameObject("Cameras").transform;

        private void SpawnCamera()
        {
            if (_cameraPrefab == null)
                return;

            var cameraTransform = _cameraPrefab.transform;
            var instance = _container.InstantiatePrefabForComponent<FollowingCamera>(
                _cameraPrefab.gameObject, cameraTransform.position, cameraTransform.rotation, _cameraGroup);

            _signalBus.Fire(new FollowingCameraSpawnedSignal { FollowingCamera = instance });
        }

        public void OnUpdate(float deltaTime)
        {
        }

        public void Dispose()
        {
        }
    }
}