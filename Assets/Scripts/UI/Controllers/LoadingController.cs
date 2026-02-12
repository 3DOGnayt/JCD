using KoboldUi.Element.Controller;
using KoboldUi.Services.WindowsService;
using Services;
using System;
using UI.Views;
using UI.Window;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI.Controllers
{
    public class LoadingController : AUiController<LoadingView>
    {
        private const int MaxProgress = 100;
        
        private readonly ILocalWindowsService _localWindowsService;
        private readonly ILoadingService _loadingService;
        private readonly IGameSessionService _gameSessionService;
        private IDisposable _loadingDisposable;

        public LoadingController(
            ILocalWindowsService localWindowsService,
            ILoadingService loadingService,
            IGameSessionService gameSessionService
        )
        {
            _localWindowsService = localWindowsService;
            _loadingService = loadingService;
            _gameSessionService = gameSessionService;
        }

        public override void Initialize() { }

        protected override void OnOpen()
        {
            StartFakeLoading();
        }

        private void StartFakeLoading()
        {
            if (View.LoadingText == null)
                return;

            _loadingService.IsLoadingCompleted.Value = false;
            _loadingService.LoadingProgress.Value = 0f;
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
                    UpdateProgress(Mathf.RoundToInt(curved * MaxProgress));
                });
            _loadingDisposable.AddTo(View);
        }

        private void UpdateProgress(int progress)
        {
            if (_loadingService.IsLoadingCompleted.Value)
                return;

            var clamped = progress > MaxProgress ? MaxProgress : progress;
            _loadingService.LoadingProgress.Value = clamped / (float)MaxProgress;
            View.LoadingText.text = FormatProgress(clamped);

            if (clamped >= MaxProgress)
                OnLoadCompleted();
        }

        private string FormatProgress(int progress)
        {
            return progress + " %";
        }

        private void OnLoadCompleted()
        {
            _loadingService.IsLoadingCompleted.Value = true;

            _loadingDisposable?.Dispose();
            _loadingDisposable = null;
            
            var target = _gameSessionService != null ? _gameSessionService.Target : GameSessionTarget.Game;
            if (target == GameSessionTarget.Game)
            {
                _loadingService.PublishStartRace();
                _localWindowsService.OpenWindow<GameWindow>();
                return;
            }

            SceneManager.LoadScene(0);
        }
    }
}
