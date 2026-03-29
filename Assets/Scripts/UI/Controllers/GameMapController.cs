using Cameras;
using Helpers.Car;
using KoboldUi.Element.Controller;
using Services;
using UI.Views;
using UniRx;
using UnityEngine;

namespace UI.Controllers
{
    public class GameMapController : AUiController<GameMapView>
    {
        private readonly IEventService _eventService;
        
        private ICarView _carView;
        private ICarView _opponentView;
        private MinimapCameraHolder _minimapCamera;

        public GameMapController(IEventService eventService)
        {
            _eventService = eventService;
        }

        public override void Initialize()
        {
            if (_eventService == null)
                return;

            _eventService.PlayerSpawnedStream.Subscribe(OnPlayerSpawned).AddTo(View);
            _eventService.OpponentSpawnedStream.Subscribe(OnOpponentSpawned).AddTo(View);
            _eventService.MinimapSpawnedStream.Subscribe(OnMinimapSpawned).AddTo(View);
            Observable.EveryUpdate().Subscribe(_ => UpdateEnemyDot()).AddTo(View);
        }

        private void OnPlayerSpawned(ICarView carView)
        {
            if (carView == null)
                return;

            _carView = carView;
        }

        private void OnMinimapSpawned(MinimapCameraHolder minimapCamera)
        {
            if (minimapCamera == null)
                return;

            _minimapCamera = minimapCamera;
            
            TryBindMinimap();
        }

        private void OnOpponentSpawned(ICarView carView)
        {
            if (carView == null)
                return;

            _opponentView = carView;
        }

        private void TryBindMinimap()
        {
            if (_minimapCamera == null || _carView == null)
                return;

            View.SetMinimapCamera(_minimapCamera);
        }

        private void UpdateEnemyDot()
        {
            if (!TryGetEnemyViewport(out var viewport, out var mapRect))
            {
                View.SetEnemyVisible(false);
                return;
            }

            var size = mapRect.rect.size;
            var anchored = new Vector2((viewport.x - 0.5f) * size.x, (viewport.y - 0.5f) * size.y);

            var halfSize = size * 0.5f;
            anchored.x = Mathf.Clamp(anchored.x, -halfSize.x, halfSize.x);
            anchored.y = Mathf.Clamp(anchored.y, -halfSize.y, halfSize.y);

            View.SetEnemyVisible(true);
            View.SetEnemyPosition(anchored);
        }

        private bool TryGetEnemyViewport(out Vector3 viewport, out RectTransform mapRect)
        {
            viewport = default;
            mapRect = View.MapRect;

            if (_minimapCamera == null || _minimapCamera.Camera == null || _opponentView == null || mapRect == null)
                return false;

            var enemyTransform = _opponentView.CarTransform;
            if (enemyTransform == null)
                return false;

            viewport = _minimapCamera.Camera.WorldToViewportPoint(enemyTransform.position);
            return viewport.z >= 0f;
        }
    }
}