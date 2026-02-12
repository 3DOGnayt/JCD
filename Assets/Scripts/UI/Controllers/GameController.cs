using KoboldUi.Element.Controller;
using KoboldUi.Services.WindowsService;
using Services;
using UI.Views;
using UI.Window;
using UniRx;
using UnityEngine;

namespace UI.Controllers
{
    public class GameController : AUiController<GameView>
    {
        private readonly ILocalWindowsService _localWindowsService;
        private readonly IRaceTimerService _raceTimerService;
        private readonly ILoadingService _loadingService;
        
        private bool _isInputUnlocked;

        public GameController(
            ILocalWindowsService localWindowsService,
            IRaceTimerService raceTimerService,
            ILoadingService loadingService
        )
        {
            _localWindowsService = localWindowsService;
            _raceTimerService = raceTimerService;
            _loadingService = loadingService;
        }

        public override void Initialize()
        {
            Observable.EveryUpdate()
                .Where(_ => Input.GetKeyDown(KeyCode.Escape))
                .Subscribe(_ => OnPauseClick())
                .AddTo(View);

            _loadingService.InputEnabledStream.Subscribe(value => _isInputUnlocked = value).AddTo(View);
            _loadingService.IsGameStarted.Subscribe(_ => OnStartGame()).AddTo(View);

            _raceTimerService.RaceFinishedStream.Subscribe(_ => ShowResult()).AddTo(View);
        }

        protected override void OnOpen()
        {
            _loadingService.PublishGameStarted(true);
        }

        private void OnStartGame()
        {
            _localWindowsService.OpenWindow<GameStartEndWindow>();
        }

        private void ShowResult()
        {
            _localWindowsService.OpenWindow<GameStartEndWindow>();
        }

        private void OnPauseClick()
        {
            if (!_isInputUnlocked)
                return;
            
            _localWindowsService.OpenWindow<GamePauseWindow>();
        }
    }
}