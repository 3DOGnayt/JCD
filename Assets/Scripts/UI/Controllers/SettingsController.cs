using KoboldUi.Element.Controller;
using KoboldUi.Services.WindowsService;
using UI.Views;
using UI.Window;
using UniRx;

namespace UI.Controllers
{
    public class SettingsController : AUiController<SettingsView>
    {
        private readonly ILocalWindowsService _localWindowsService;

        public SettingsController(ILocalWindowsService localWindowsService)
        {
            _localWindowsService = localWindowsService;
        }

        public override void Initialize()
        {
            View.CloseButton.OnClickAsObservable().Subscribe(_ => OnCloseButtonClick()).AddTo(View);
        }

        private void OnCloseButtonClick() => _localWindowsService.OpenWindow<MainMenuWindow>();
    }
}