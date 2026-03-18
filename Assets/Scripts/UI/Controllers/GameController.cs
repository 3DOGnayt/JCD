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
        private readonly IEventService _eventService;
        private readonly IAudioService _audioService;
        
        private bool _isInputUnlocked;

        public GameController(
            ILocalWindowsService localWindowsService,
            IRaceTimerService raceTimerService,
            IEventService eventService,
            IAudioService audioService
        )
        {
            _localWindowsService = localWindowsService;
            _raceTimerService = raceTimerService;
            _eventService = eventService;
            _audioService = audioService;
        }

        public override void Initialize()
        {
            Observable.EveryUpdate()
                .Where(_ => Input.GetKeyDown(KeyCode.Escape))
                .Subscribe(_ => OnPauseClick())
                .AddTo(View);

            _eventService.InputEnabledStream.Subscribe(value => _isInputUnlocked = value).AddTo(View);
            _eventService.IsGameStarted.Subscribe(OnStartGame).AddTo(View);

            _raceTimerService.RaceFinishedStream.Subscribe(_ => OnRaceFinished()).AddTo(View);
        }

        protected override void OnOpen()
        {
            _eventService.PublishGameStarted(true);
        }

        private void OnStartGame(bool value)
        {
            if (!value)
                return;

            _localWindowsService.OpenWindow<GameStartEndWindow>();
        }

        private void ShowResult()
        {
            _localWindowsService.OpenWindow<GameStartEndWindow>();
        }

        private void OnRaceFinished()
        {
            _audioService?.StopMusic();
            _audioService?.StopAllSfx();
            ShowResult();
        }

        private void OnPauseClick()
        {
            if (!_isInputUnlocked)
                return;

            if (_localWindowsService.IsOpened<SettingsWindow>())
                return;

            _localWindowsService.OpenWindow<GamePauseWindow>();
        }
    }
}