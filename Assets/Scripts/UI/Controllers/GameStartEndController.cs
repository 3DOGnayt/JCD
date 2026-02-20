using Configs.Impl;
using Data.Enums;
using KoboldUi.Element.Controller;
using KoboldUi.Services.WindowsService;
using DG.Tweening;
using UI.Views;
using UI.Window;
using Services;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.Controllers
{
    public class GameStartEndController : AUiController<GameStartEndView>
    {
        private readonly ILoadingService _loadingService;
        private readonly IRaceTimerService _raceTimerService;
        private readonly ILocalWindowsService _localWindowsService;
        private readonly IAudioService _audioService;

        private Sequence _countdownSequence;
        private Tween _winTween;
        private Sequence _resultSequence;
        
        [Inject] private GameSelectionParameters _gameSelectionParameters;
        
        public GameStartEndController(
            ILoadingService loadingService,
            IRaceTimerService raceTimerService,
            ILocalWindowsService localWindowsService,
            IAudioService audioService
        )
        {
            _loadingService = loadingService;
            _raceTimerService = raceTimerService;
            _localWindowsService = localWindowsService;
            _audioService = audioService;
        }

        public override void Initialize() { }

        protected override void OnOpen()
        {
            _loadingService?.PublishInputEnabled(false);
            
            if (_raceTimerService.IsFinished)
            {
                ShowResult();
                return;
            }
            
            SetStateImages();
            PlayCountdown();
        }

        private void SetStateImages()
        {
            if (View.СountdownList == null)
                return;
            
            View.Win.gameObject.SetActive(false);
            SetImageAlpha(View.Win, 0f);
            
            View.Lose.gameObject.SetActive(false);
            SetImageAlpha(View.Lose, 0f);
            
            for (var i = 0; i < View.СountdownList.Count; i++)
            {
                var image = View.СountdownList[i];
                if (image == null)
                    continue;

                image.gameObject.SetActive(false);
                SetImageAlpha(image, 0f);
            }
        }

        private void PlayCountdown()
        {
            _countdownSequence?.Kill();
            _countdownSequence = DOTween.Sequence().SetLink(View.gameObject);

            var startDelay = View.CountdownStartDelaySeconds;
            if (startDelay > 0f)
                _countdownSequence.AppendInterval(startDelay);

            if (View.СountdownList == null || View.СountdownList.Count == 0)
            {
                _countdownSequence.AppendCallback(PublishCountdownFinished);
                return;
            }

            var fadeIn = View.CountdownFadeInSeconds;
            var fadeOut = View.CountdownFadeOutSeconds;

            _audioService.PlayMusicAudio(EAudioType.Music, _gameSelectionParameters.SelectedMusicSubType, 0.05f); // TODO: SOUND
            _audioService.PlaySfx2DAudio(EAudioType.Ui, EAudioSubType.Ui_3); // TODO: SOUND
            
            for (var i = 0; i < View.СountdownList.Count; i++)
            {
                var viewСountdown = View.СountdownList[i];
                if (viewСountdown == null)
                    continue;

                var hold = GetHoldSeconds(i);

                _countdownSequence.AppendCallback(() =>
                {
                    viewСountdown.gameObject.SetActive(true);
                    SetImageAlpha(viewСountdown, fadeIn > 0f ? 0f : 1f);
                });

                if (fadeIn > 0f)
                    _countdownSequence.Append(viewСountdown.DOFade(1f, fadeIn));

                if (hold > 0f)
                    _countdownSequence.AppendInterval(hold);

                if (fadeOut > 0f)
                {
                    _countdownSequence.Append(viewСountdown.DOFade(0f, fadeOut));
                    _countdownSequence.AppendCallback(() => { viewСountdown.gameObject.SetActive(false); });
                }
                else
                    _countdownSequence.AppendCallback(() => { viewСountdown.gameObject.SetActive(false); });
            }

            _countdownSequence.AppendCallback(PublishCountdownFinished);
            _countdownSequence.AppendCallback(() => _loadingService?.PublishInputEnabled(true));
            _countdownSequence.AppendCallback(() => _audioService.PlaySfx2DAudio(EAudioType.Ui, EAudioSubType.Ui_Start));
            _countdownSequence.AppendCallback(() => _localWindowsService.CloseWindow());
        }

        private float GetHoldSeconds(int index)
        {
            if (View.CountdownHoldSeconds != null && index >= 0 && index < View.CountdownHoldSeconds.Count)
                return Mathf.Max(0f, View.CountdownHoldSeconds[index]);

            return Mathf.Max(0f, View.CountdownHoldSecondsDefault);
        }

        private void ShowResult()
        {
            _countdownSequence?.Kill();
            _winTween?.Kill();
            _resultSequence?.Kill();

            HideCountdownImages();

            //todo: game result
            // var result = _gameResultParameters.Result;
            // if (result == EGameResult.Win)
            // {
            //     View.Lose.gameObject.SetActive(false);
            //
            //     ShowResultImage(View.Win, View.WinFadeInSeconds);
            // }
            // else if (result == EGameResult.Lose)
            // {
            //     View.Win.gameObject.SetActive(false);
            //
            //     ShowResultImage(View.Lose, View.WinFadeInSeconds);
            // }
            
            _audioService.PlayUiAudio(EAudioType.Ui, EAudioSubType.Ui_Win); // TODO: SOUND
            
            View.Win.gameObject.SetActive(true);
            ShowResultImage(View.Win, View.WinFadeInSeconds);
            _loadingService.PublishInputEnabled(false);
            _loadingService.PublishWinResultChanged(true);
            
            StartResultFlow();
        }

        private void HideCountdownImages()
        {
            if (View.СountdownList == null)
                return;

            for (var i = 0; i < View.СountdownList.Count; i++)
            {
                var image = View.СountdownList[i];
                if (image == null)
                    continue;

                image.gameObject.SetActive(false);
            }
        }

        private static void SetImageAlpha(Image image, float alpha)
        {
            if (image == null)
                return;

            var color = image.color;
            color.a = alpha;
            image.color = color;
        }

        private void PublishCountdownFinished()
        {
            if (_loadingService == null)
                return;

            _loadingService.PublishCountdownFinished();
        }

        private void ShowResultImage(Image image, float fadeInSeconds)
        {
            if (image == null)
                return;

            image.gameObject.SetActive(true);
            var fadeIn = Mathf.Max(0f, fadeInSeconds);
            if (fadeIn <= 0f)
            {
                SetImageAlpha(image, 1f);
                return;
            }

            SetImageAlpha(image, 0f);
            _winTween = image.DOFade(1f, fadeIn).SetLink(image.gameObject);
        }

        private void StartResultFlow()
        {
            var delay = Mathf.Max(0f, View.WinFadeInSeconds) + Mathf.Max(0f, View.ResultHoldSeconds);
            _resultSequence?.Kill();
            _resultSequence = DOTween.Sequence().SetLink(View.gameObject);
            _resultSequence.AppendInterval(delay);
            _resultSequence.AppendCallback(CompleteResultFlow);
        }

        private void CompleteResultFlow()
        {
            _localWindowsService.CloseToWindow<GameWindow>();
            _localWindowsService.OpenWindow<GameResultWindow>();
        }
    }
}