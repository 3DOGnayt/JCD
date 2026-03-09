using Cameras;
using Helpers.CarView;
using KoboldUi.Element.Controller;
using Services;
using UI.Views;
using UniRx;

namespace UI.Controllers
{
    public class GameMapController : AUiController<GameMapView>
    {
        private readonly IEventService _eventService;
        
        private ICarView _carView;
        private MinimapCameraHolder _minimapCamera;

        public GameMapController(
            IEventService eventService
        )
        {
            _eventService = eventService;
        }

        public override void Initialize()
        {
            if (_eventService == null)
                return;

            _eventService.PlayerSpawnedStream.Subscribe(OnPlayerSpawned).AddTo(View);
            _eventService.MinimapSpawnedStream.Subscribe(OnMinimapSpawned).AddTo(View);
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

        private void TryBindMinimap()
        {
            if (_minimapCamera == null || _carView == null)
                return;

            View.SetMinimapCamera(_minimapCamera);
        }
    }
}