using Cinemachine;
using Helpers;
using Scellecs.Morpeh;
using UI;
using UnityEngine;
using Zenject;

namespace Systems.Spawn
{
    public sealed class CameraSpawnSystem : ISystem
    {
        [Inject] public World World { get; set; }
        [Inject] private FollowingCamera _cameraPrefab;
        [Inject] private CinemachineFreeLook _cinemachineFreeLookPrefab;
        [Inject] private DiContainer _container;

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
            
            if (_cinemachineFreeLookPrefab == null)
                return;

            var cameraTransform = _cameraPrefab.transform;
            _container.InstantiatePrefabForComponent<FollowingCamera>(
                _cameraPrefab.gameObject,
                cameraTransform.position,
                cameraTransform.rotation,
                _cameraGroup);
            
            var cinemachineTransform = _cinemachineFreeLookPrefab.transform;
            _container.InstantiatePrefabForComponent<CinemachineFreeLook>(
                _cinemachineFreeLookPrefab.gameObject,
                cinemachineTransform.position,
                cinemachineTransform.rotation,
                _cameraGroup);
        }

        public void OnUpdate(float deltaTime) { }

        public void Dispose() { }
    }
}