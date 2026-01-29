using KoboldUi.Element.Controller;
using KoboldUi.Services.WindowsService;
using UI.Views;
using UI.Window;
using UniRx;
using UnityEngine;

namespace UI.Controllers
{
    public class MainMenuController : AUiController<MainMenuView>
    {
        private readonly ILocalWindowsService _localWindowsService;

        public MainMenuController(ILocalWindowsService localWindowsService)
        {
            _localWindowsService = localWindowsService;
        }

        public override void Initialize()
        {
            View.StartButton.OnClickAsObservable().Subscribe(_ => OnStartButtonClick()).AddTo(View);
            View.GarageButton.OnClickAsObservable().Subscribe(_ => OnGarageButtonClick()).AddTo(View);
            View.SettingsButton.OnClickAsObservable().Subscribe(_ => OnSettingsButtonClick()).AddTo(View);
            View.ExitButton.OnClickAsObservable().Subscribe(_ => OnExitButtonClick()).AddTo(View);
        }

        private void OnStartButtonClick() => _localWindowsService.OpenWindow<MapWindow>();
        private void OnGarageButtonClick()=> _localWindowsService.OpenWindow<GarageWindow>();
        private void OnSettingsButtonClick() => _localWindowsService.OpenWindow<SettingsWindow>();
        private void OnExitButtonClick() => Application.Quit();
    }
}
