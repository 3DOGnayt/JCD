using KoboldUi.Element.Controller;
using Configs.Impl;
using Services;
using UI.Views;
using UniRx;
using Views;

namespace UI.Controllers
{
    public class GameMapController : AUiController<GameMapView>
    {
        private readonly ILoadingService _loadingService;
        private readonly MapCatalogParameters _mapCatalogParameters;
        private readonly GameSelectionParameters _gameSelectionParameters;

        public GameMapController(
            ILoadingService loadingService,
            MapCatalogParameters mapCatalogParameters,
            GameSelectionParameters gameSelectionParameters)
        {
            _loadingService = loadingService;
            _mapCatalogParameters = mapCatalogParameters;
            _gameSelectionParameters = gameSelectionParameters;
        }

        public override void Initialize()
        {
            if (_loadingService == null)
                return;

            _loadingService.PlayerSpawnedStream.Subscribe(OnPlayerSpawned).AddTo(View);

            Observable.EveryUpdate()
                .Subscribe(_ => View.UpdateMap())
                .AddTo(View);

            ApplySelectedMapSettings();
        }

        protected override void OnOpen()
        {
            ApplySelectedMapSettings();
        }

        private void OnPlayerSpawned(ICarView carView)
        {
            if (carView == null)
                return;

            View.SetPlayer(carView.CarTransform);
        }

        private void ApplySelectedMapSettings()
        {
            if (_mapCatalogParameters == null || _gameSelectionParameters == null)
                return;

            var index = _gameSelectionParameters.SelectedMapIndex;
            if (index < 0 || index >= _mapCatalogParameters.Maps.Count)
                return;

            var entry = _mapCatalogParameters.Maps[index];
            View.ApplySettings(entry.MiniMap);
        }
    }
}