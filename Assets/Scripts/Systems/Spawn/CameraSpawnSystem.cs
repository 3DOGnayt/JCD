using System;
using Cinemachine;
using Helpers;
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
        [Inject] private CinemachineFreeLook _cinemachineFreeLookPrefab;
        [Inject] private DiContainer _container;
        [Inject] private IEventService _eventService;

        private Transform _cameraGroup;
        
        private FollowingCamera _followingCameraInstance;
        private Vector3 _followingInitialPosition;
        private Quaternion _followingInitialRotation;
        
        private CinemachineFreeLook _cinemachineFreeLookInstance;
        private Vector3 _cinemachineInitialPosition;
        private Quaternion _cinemachineInitialRotation;
        
        private IDisposable _startRaceDisposable;

        public void OnAwake()
        {
            SetSpawnRoot();
            SpawnCamera();
            SubscribeToRaceStart();
        }

        private void SetSpawnRoot() => _cameraGroup = new GameObject("Cameras").transform;

        private void SpawnCamera()
        {
            if (_cameraPrefab == null)
                return;
            
            if (_cinemachineFreeLookPrefab == null)
                return;

            var cameraTransform = _cameraPrefab.transform;
            _followingCameraInstance = _container.InstantiatePrefabForComponent<FollowingCamera>(
                _cameraPrefab.gameObject,
                cameraTransform.position,
                cameraTransform.rotation,
                _cameraGroup);
            
            _followingInitialPosition = _followingCameraInstance.transform.position;
            _followingInitialRotation = _followingCameraInstance.transform.rotation;
            
            var cinemachineTransform = _cinemachineFreeLookPrefab.transform;
            _cinemachineFreeLookInstance = _container.InstantiatePrefabForComponent<CinemachineFreeLook>(
                _cinemachineFreeLookPrefab.gameObject,
                cinemachineTransform.position,
                cinemachineTransform.rotation,
                _cameraGroup);
            
            _cinemachineInitialPosition = _cinemachineFreeLookInstance.transform.position;
            _cinemachineInitialRotation = _cinemachineFreeLookInstance.transform.rotation;
        }

        private void SubscribeToRaceStart()
        {
            if (_eventService == null)
                return;

            _startRaceDisposable = _eventService.StartRaceStream.Subscribe(_ => ResetCameraPositions());
        }

        private void ResetCameraPositions()
        {
            if (_followingCameraInstance != null)
            {
                _followingCameraInstance.transform.position = _followingInitialPosition;
                _followingCameraInstance.transform.rotation = _followingInitialRotation;
            }

            if (_cinemachineFreeLookInstance != null)
            {
                _cinemachineFreeLookInstance.transform.position = _cinemachineInitialPosition;
                _cinemachineFreeLookInstance.transform.rotation = _cinemachineInitialRotation;
            }
        }

        public void OnUpdate(float deltaTime) { }

        public void Dispose()
        {
            _startRaceDisposable?.Dispose();
        }
    }
}