using KoboldUi.Element.Controller;
using Configs.Impl;
using Helpers.CarView;
using Services;
using UI.Views;
using UniRx;

namespace UI.Controllers
{
    public class GameMapController : AUiController<GameMapView>
    {
        private readonly ILoadingService _loadingService;
        private readonly MapCatalogParameters _mapCatalogParameters;
        private readonly MapSelectionParameters _mapSelectionParameters;

        public GameMapController(
            ILoadingService loadingService,
            MapCatalogParameters mapCatalogParameters,
            MapSelectionParameters mapSelectionParameters)
        {
            _loadingService = loadingService;
            _mapCatalogParameters = mapCatalogParameters;
            _mapSelectionParameters = mapSelectionParameters;
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
            if (_mapCatalogParameters == null || _mapSelectionParameters == null)
                return;

            var index = _mapSelectionParameters.SelectedMapIndex;
            if (index < 0 || index >= _mapCatalogParameters.Maps.Count)
                return;

            var entry = _mapCatalogParameters.Maps[index];
            View.ApplySettings(entry.MiniMap);
        }
    }
}