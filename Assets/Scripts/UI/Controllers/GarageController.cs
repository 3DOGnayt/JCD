using KoboldUi.Element.Controller;
using KoboldUi.Services.WindowsService;
using UI.Views;
using UI.Window;
using UniRx;

namespace UI.Controllers
{
    public class GarageController : AUiController<GarageView>
    {
        private ILocalWindowsService _localWindowsService;

        public GarageController(ILocalWindowsService localWindowsService)
        {
            _localWindowsService = localWindowsService;
        }

        public override void Initialize()
        {
            for (var i = 0; i < View.CarButtons.Count; i++)
            {
                View.CarButtons[i].OnClickAsObservable().Subscribe(_ => OnCarButtonClick(i)).AddTo(View); // save car
            }

            //View.CarPresentation.sprite = _carCatalog... // last save

            View.ConfirmButton.OnClickAsObservable().Subscribe(_ => OnConfirmButtonClick()).AddTo(View);
            View.BackButton.OnClickAsObservable().Subscribe(_ => OnBackButtonClick()).AddTo(View);
        }

        private void OnCarButtonClick(int index)
        {
            //View.CarPresentation.sprite = _carCatalog... // new sprite by index
        }

        private void OnConfirmButtonClick() => _localWindowsService.OpenWindow<MainMenuWindow>();
        private void OnBackButtonClick() => _localWindowsService.OpenWindow<MainMenuWindow>();
    }
}