using KoboldUi.Element.Controller;
using KoboldUi.Services.WindowsService;
using Services;
using UI.Views;
using UI.Window;
using UniRx;
using UnityEngine;

namespace UI.Controllers
{
    public class GamePauseController : AUiController<GamePauseView>
    {
        private readonly ILocalWindowsService _localWindowsService;
        private readonly IGameSessionService _gameSessionService;

        public GamePauseController(
            ILocalWindowsService localWindowsService,
            IGameSessionService gameSessionService
        )
        {
            _localWindowsService = localWindowsService;
            _gameSessionService = gameSessionService;
        }

        public override void Initialize()
        {
            View.Continue.OnClickAsObservable().Subscribe(_ => OnContinueClick()).AddTo(View);
            View.Retry.OnClickAsObservable().Subscribe(_ => OnRetryClick()).AddTo(View);
            View.Menu.OnClickAsObservable().Subscribe(_ => OnMainMenuClick()).AddTo(View);
            View.Settings.OnClickAsObservable().Subscribe(_ => OnSettingsClick()).AddTo(View);
        }

        protected override void OnOpen()
        {
            Time.timeScale = 0f;
        }

        private void OnContinueClick()
        {
            Time.timeScale = 1f;
            _localWindowsService.CloseToWindow<GameWindow>();
        }
        
        private void OnRetryClick()
        {
            Time.timeScale = 1f;
            _gameSessionService?.RestartGame();
        }

        private void OnMainMenuClick()
        {
            Time.timeScale = 1f;
            _gameSessionService?.ExitToMenu();
        }

        private void OnSettingsClick()
        {
            _localWindowsService.OpenWindow<SettingsWindow>();
        }
    }
}