using KoboldUi.Element.Controller;
using KoboldUi.Services.WindowsService;
using UI.Views;
using UI.Window;
using UniRx;

namespace UI.Controllers
{
    public class MapController : AUiController<MapView>
    {
        private ILocalWindowsService _localWindowsService;

        public MapController(ILocalWindowsService localWindowsService)
        {
            _localWindowsService = localWindowsService;
        }

        public override void Initialize()
        {
            for (var i = 0; i < View.MapButtons.Count; i++)
            {
                View.MapButtons[i].OnClickAsObservable().Subscribe(_ => OnMapButtonClick(i)).AddTo(View); // save map 
            }

            //View.MapPresentation.sprite = _mapCatalog... // last save

            View.ConfirmButton.OnClickAsObservable().Subscribe(_ => OnConfirmButtonClick()).AddTo(View);
            View.BackButton.OnClickAsObservable().Subscribe(_ => OnBackButtonClick()).AddTo(View);
        }

        private void OnMapButtonClick(int index)
        {
            //View.MapPresentation.sprite = _mapCatalog... // new sprite by index
        }

        private void OnConfirmButtonClick() => _localWindowsService.OpenWindow<OpponentWindow>();
        private void OnBackButtonClick() => _localWindowsService.OpenWindow<MainMenuWindow>();
    }
}