using KoboldUi.Element.Controller;
using KoboldUi.Services.WindowsService;
using UI.Views;
using UI.Window;
using UniRx;

namespace UI.Controllers
{
    public class OpponentController : AUiController<OpponentView>
    {
        private ILocalWindowsService _localWindowsService;

        public OpponentController(ILocalWindowsService localWindowsService)
        {
            _localWindowsService = localWindowsService;
        }

        public override void Initialize()
        {
            for (var i = 0; i < View.OpponentButtons.Count; i++)
            {
                View.OpponentButtons[i].OnClickAsObservable().Subscribe(_ => OnOpponentButtonClick(i)).AddTo(View); // save opponent 
            }

            //View.OpponentPresentation.sprite = _opponentCatalog... // last save
            //View.OpponentDifficulty.fillAmount = _opponentCatalog... // last save

            View.ConfirmButton.OnClickAsObservable().Subscribe(_ => OnConfirmButtonClick()).AddTo(View);
            View.BackButton.OnClickAsObservable().Subscribe(_ => OnBackButtonClick()).AddTo(View);
        }

        private void OnOpponentButtonClick(int index)
        {
            //View.OpponentPresentation.sprite = _opponentCatalog... // new sprite by index
            //View.OpponentDifficulty.fillAmount = _opponentCatalog... // new dif by index
        }

        private void OnConfirmButtonClick() => _localWindowsService.OpenWindow<RaceWindow>();
        private void OnBackButtonClick() => _localWindowsService.OpenWindow<MapWindow>();
    }
}