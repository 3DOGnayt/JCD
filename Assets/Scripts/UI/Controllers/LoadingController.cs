using KoboldUi.Element.Controller;
using KoboldUi.Services.WindowsService;
using Signals;
using System;
using UI.Views;
using UI.Window;
using UniRx;
using Zenject;

namespace UI.Controllers
{
    public class LoadingController : AUiController<LoadingView>
    {
        private const int MaxProgress = 100;
        private const float FakeLoadingDurationSeconds = 3.5f;
        
        private readonly ILocalWindowsService _localWindowsService;
        private IDisposable _loadingDisposable;
        
        [Inject] private SignalBus _signalBus;

        public LoadingController(ILocalWindowsService localWindowsService)
        {
            _localWindowsService = localWindowsService;
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

            View.LoadingText.text = FormatProgress(0);

            var stepInterval = FakeLoadingDurationSeconds / MaxProgress;
            _loadingDisposable?.Dispose();
            _loadingDisposable = Observable.Interval(TimeSpan.FromSeconds(stepInterval))
                .Select(index => (int)index + 1)
                .Take(MaxProgress)
                .Subscribe(UpdateProgress);
            _loadingDisposable.AddTo(View);
        }

        private void UpdateProgress(int progress)
        {
            var clamped = progress > MaxProgress ? MaxProgress : progress;
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
            _signalBus.Fire(new StartRaceSignal());
            
            _localWindowsService.CloseToWindow<MainMenuWindow>();
            _localWindowsService.OpenWindow<GameWindow>();
        }
    }
}