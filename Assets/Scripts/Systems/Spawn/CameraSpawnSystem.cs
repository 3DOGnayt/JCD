using System;
using Cinemachine;
using Helpers;
using Helpers.Car;
using Scellecs.Morpeh;
using Services;
using UniRx;
using UnityEngine;
using Zenject;

namespace Systems.Spawn
{
    public sealed class CameraSpawnSystem : ISystem
    {
        [Inject] public World World { get; set; }
        [Inject] private FollowingCamera _cameraPrefab;
        [Inject] private DiContainer _container;
        [Inject] private IEventService _eventService;

        private Transform _cameraGroup;
        
        private FollowingCamera _followingCameraInstance;
        private Vector3 _followingInitialPosition;
        private Quaternion _followingInitialRotation;
        private CinemachineVirtualCamera _virtualCamera;
        
        private IDisposable _startRaceDisposable;
        private IDisposable _playerSpawnedDisposable;
        private readonly SerialDisposable _followDelayDisposable = new();

        public void OnAwake()
        {
            SetSpawnRoot();
            SpawnCamera();
            SubscribeToPlayerSpawned();
        }

        private void SetSpawnRoot() => _cameraGroup = new GameObject("Cameras").transform;

        private void SpawnCamera()
        {
            if (_cameraPrefab == null)
                return;

            var cameraTransform = _cameraPrefab.transform;
            _followingCameraInstance = _container.InstantiatePrefabForComponent<FollowingCamera>(
                _cameraPrefab.gameObject,
                cameraTransform.position,
                cameraTransform.rotation,
                _cameraGroup);
            
            _followingInitialPosition = _followingCameraInstance.transform.position;
            _followingInitialRotation = _followingCameraInstance.transform.rotation;
            _virtualCamera = _followingCameraInstance.GetComponent<CinemachineVirtualCamera>();
        }

        private void SubscribeToPlayerSpawned()
        {
            if (_eventService == null)
                return;

            _playerSpawnedDisposable = _eventService.PlayerSpawnedStream.Subscribe(OnPlayerSpawned);
        }

        private void OnPlayerSpawned(ICarView carView)
        {
            if (_virtualCamera == null || carView == null)
                return;

            var target = carView.CarTransform;
            ResetVirtualCameraTarget(target);
        }

        private void ResetVirtualCameraTarget(Transform target)
        {
            _virtualCamera.Follow = null;
            _virtualCamera.LookAt = null;

            _followingCameraInstance.transform.position = _followingInitialPosition;
            _followingCameraInstance.transform.rotation = _followingInitialRotation;
            
            _followDelayDisposable.Disposable = Observable.NextFrame()
                .Subscribe(_ =>
                {
                    _virtualCamera.Follow = target;
                    _virtualCamera.LookAt = target;
                });
        }

        public void OnUpdate(float deltaTime) { }

        public void Dispose()
        {
            _startRaceDisposable?.Dispose();
            _playerSpawnedDisposable?.Dispose();
            _followDelayDisposable.Dispose();
        }
    }
}