using Components;
using Scellecs.Morpeh;
using UnityEngine;
using Zenject;

namespace Systems.Spawn
{
    public sealed class MinimapFollowSystem : ISystem
    {
        [Inject] public World World { get; set; }

        private static readonly Vector3 FlatForward = Vector3.forward;

        private Filter _players;
        private Filter _minimapCameras;
        private Stash<TransformComponent> _playerStash;
        private Stash<TransformComponent> _minimapCameraStash;

        private Vector3 _offset;
        private bool _offsetIsSet;
        
        //TODO
        private float _positionSmoothSpeed = 5f;
        private float _rotationSmoothSpeed = 5f;
        private float _cameraRotationX = 90f;

        public void OnAwake()
        {
            _players = World.Filter
                .With<PlayerTagComponent>()
                .Build();

            _minimapCameras = World.Filter
                .With<MiniMapTagComponent>()
                .Build();

            _playerStash = World.GetStash<TransformComponent>();
            _minimapCameraStash = World.GetStash<TransformComponent>();
        }

        public void OnUpdate(float deltaTime)
        {
            if (_players.IsEmpty() || _minimapCameras.IsEmpty())
                return;

            var playerEntity = _players.First();
            var cameraEntity = _minimapCameras.First();

            ref var playerTransform = ref _playerStash.Get(playerEntity);
            ref var cameraTransform = ref _minimapCameraStash.Get(cameraEntity);

            var player = playerTransform.Value;
            var miniMapCamera = cameraTransform.Value;

            if (!_offsetIsSet) 
            {
                _offset = miniMapCamera.position - player.position;
                _offsetIsSet = true;
            }

            var targetPosition = player.position + _offset;
            var positionT = 1f - Mathf.Exp(-_positionSmoothSpeed * deltaTime);
            miniMapCamera.position = Vector3.Lerp(miniMapCamera.position, targetPosition, positionT);

            var playerForward = player.rotation * FlatForward;
            playerForward.y = 0f;
            if (playerForward.sqrMagnitude < 0.0001f)
                playerForward = FlatForward;

            var yaw = Quaternion.LookRotation(playerForward, Vector3.up).eulerAngles.y;
            var targetRotation = Quaternion.Euler(_cameraRotationX, yaw, 0f);
            var rotationT = 1f - Mathf.Exp(-_rotationSmoothSpeed * deltaTime);
            miniMapCamera.rotation = Quaternion.Slerp(miniMapCamera.rotation, targetRotation, rotationT);
        }

        public void Dispose() { }
    }
}