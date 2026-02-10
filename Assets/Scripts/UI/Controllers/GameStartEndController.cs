using KoboldUi.Element.Controller;
using KoboldUi.Services.WindowsService;
using DG.Tweening;
using UI.Views;
using UI.Window;
using Services;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Controllers
{
    public class GameStartEndController : AUiController<GameStartEndView>
    {
        private readonly ILoadingService _loadingService;
        private readonly IRaceTimerService _raceTimerService;
        private readonly ILocalWindowsService _localWindowsService;
        
        private Sequence _countdownSequence;
        private Tween _winTween;

        public GameStartEndController(
            ILoadingService loadingService,
            IRaceTimerService raceTimerService,
            ILocalWindowsService localWindowsService)
        {
            _loadingService = loadingService;
            _raceTimerService = raceTimerService;
            _localWindowsService = localWindowsService;
        }

        public override void Initialize()
        {
            if (_raceTimerService == null)
                return;

            _raceTimerService.RaceFinishedStream.Subscribe(_ => ShowResult()).AddTo(View);
        }

        protected override void OnOpen()
        {
            _loadingService?.PublishInputEnabled(false);
            SetStateImages();
            
            if (_raceTimerService != null && _raceTimerService.IsFinished)
            {
                ShowResult();
                return;
            }

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
            
            View.Win.gameObject.SetActive(false);

            ShowResultImage(View.Lose, View.WinFadeInSeconds);
            
            _loadingService.PublishWinResultChanged(true);

            _loadingService?.PublishInputEnabled(false);
            _localWindowsService?.OpenWindow<GameResultWindow>();
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
    }
}