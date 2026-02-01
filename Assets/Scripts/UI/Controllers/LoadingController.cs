using KoboldUi.Element.Controller;
using KoboldUi.Services.WindowsService;
using Signals;
using UI.Views;
using UI.Window;
using Zenject;

namespace UI.Controllers
{
    public class LoadingController : AUiController<LoadingView>
    {
        private readonly ILocalWindowsService _localWindowsService;
        [Inject] private SignalBus _signalBus;

        public LoadingController(ILocalWindowsService localWindowsService)
        {
            _localWindowsService = localWindowsService;
        }

        public override void Initialize()
        {
            View.LoadingText.text = OnChangeLoadingText();
        }

        private string OnChangeLoadingText()
        {
            return "0 %";
        }

        private void OnTrainingButtonClick()
        {
            //change game mod
        }

        private void OnLoadCompleted()
        {
            _signalBus.Fire(new StartRaceSignal());
        }

        private void OnBackButtonClick() => _localWindowsService.OpenWindow<GameMapWindow>();
    }
}