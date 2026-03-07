using Cameras;
using KoboldUi.Element.Controller;
using Helpers.CarView;
using Services;
using UI.Views;
using UniRx;

namespace UI.Controllers
{
    public class GameMapController : AUiController<GameMapView>
    {
        private readonly ILoadingService _loadingService;
        private readonly MinimapSpawner _minimapSpawner;
        private MinimapCameraController _minimapCameraInstance;

        public GameMapController(
            ILoadingService loadingService,
            MinimapSpawner minimapSpawner)
        {
            _loadingService = loadingService;
            _minimapSpawner = minimapSpawner;
        }

        public override void Initialize()
        {
            if (_loadingService == null)
                return;

            _loadingService.PlayerSpawnedStream.Subscribe(OnPlayerSpawned).AddTo(View);
        }

        private void OnPlayerSpawned(ICarView carView)
        {
            if (carView == null)
                return;

            if (_minimapCameraInstance == null && _minimapSpawner != null)
                _minimapCameraInstance = _minimapSpawner.SpawnAttached(carView.CarTransform);

            if (_minimapCameraInstance != null)
                View.SetMinimapCamera(_minimapCameraInstance);
        }
    }
}