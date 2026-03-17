using KoboldUi.Element.Controller;
using KoboldUi.Services.WindowsService;
using Services;
using System;
using Data.Enums;
using UI.Views;
using UI.Window;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI.Controllers
{
    public class LoadingController : AUiController<LoadingView>
    {
        private const int MAX_PROGRESS = 100;
        
        private readonly ILocalWindowsService _localWindowsService;
        private readonly IEventService _eventService;
        private readonly IGameSessionService _gameSessionService;
        private readonly IAudioService _audioService;
        
        private IDisposable _loadingDisposable;

        public LoadingController(
            ILocalWindowsService localWindowsService,
            IEventService eventService,
            IGameSessionService gameSessionService,
            IAudioService audioService
        )
        {
            _localWindowsService = localWindowsService;
            _eventService = eventService;
            _gameSessionService = gameSessionService;
            _audioService = audioService;
        }

        public override void Initialize() { }

        protected override void OnOpen()
        {
            if (_eventService.IsGameStarted.Value)
                return;

            StartFakeLoading();
        }

        private void StartFakeLoading()
        {
            if (View.LoadingText == null)
                return;

            _audioService.StopMusic();
            _audioService.StopAllSfx();
            
            _eventService.IsLoadingCompleted.Value = false;
            _eventService.LoadingProgress.Value = 0f;
            View.LoadingText.text = FormatProgress(0);

            _loadingDisposable?.Dispose();
            var duration = View.FakeLoadingDurationSeconds <= 0f ? 0.01f : View.FakeLoadingDurationSeconds;
            var curve = View.LoadingCurve ?? AnimationCurve.Linear(0f, 0f, 1f, 1f);
            var elapsed = 0f;
            _loadingDisposable = Observable.EveryUpdate()
                .Subscribe(_ =>
                {
                    elapsed += Time.deltaTime;
                    var t = Mathf.Clamp01(elapsed / duration);
                    var curved = Mathf.Clamp01(curve.Evaluate(t));
                    UpdateProgress(Mathf.RoundToInt(curved * MAX_PROGRESS));
                });
            _loadingDisposable.AddTo(View);
        }

        private void UpdateProgress(int progress)
        {
            if (_eventService.IsLoadingCompleted.Value)
                return;

            var clamped = progress > MAX_PROGRESS ? MAX_PROGRESS : progress;
            _eventService.LoadingProgress.Value = clamped / (float)MAX_PROGRESS;
            View.LoadingText.text = FormatProgress(clamped);

            if (clamped >= MAX_PROGRESS)
                OnLoadCompleted();
        }

        private string FormatProgress(int progress)
        {
            return progress + " %";
        }

        private void OnLoadCompleted()
        {
            _eventService.IsLoadingCompleted.Value = true;

            _loadingDisposable?.Dispose();
            _loadingDisposable = null;
            
            var target = _gameSessionService != null ? _gameSessionService.Target : EGameSessionTarget.Game;
            if (target == EGameSessionTarget.Game)
            {
                _eventService.PublishStartRace();
                _localWindowsService.OpenWindow<GameWindow>();
                return;
            }

            SceneManager.LoadScene(0);
        }
    }
}