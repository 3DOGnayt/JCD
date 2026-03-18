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
        private readonly IEventService _eventService;
        private readonly IRaceTimerService _raceTimerService;
        private readonly ILocalWindowsService _localWindowsService;
        private readonly IAudioService _audioService;

        private Sequence _countdownSequence;
        private Tween _winTween;
        private Sequence _resultSequence;
        
        [Inject] private AudioSelectionParameters _audioSelectionParameters;
        
        public GameStartEndController(
            IEventService eventService,
            IRaceTimerService raceTimerService,
            ILocalWindowsService localWindowsService,
            IAudioService audioService
        )
        {
            _eventService = eventService;
            _raceTimerService = raceTimerService;
            _localWindowsService = localWindowsService;
            _audioService = audioService;
        }

        public override void Initialize() { }

        protected override void OnOpen()
        {
            _eventService?.PublishInputEnabled(false);
            
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
            
            View.Finish.gameObject.SetActive(false);
            SetImageAlpha(View.Finish, 0f);
            
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
                FinishCountdown();
                return;
            }

            var fadeIn = View.CountdownFadeInSeconds;
            var fadeOut = View.CountdownFadeOutSeconds;

            _audioService.PlayMusicAudio(EAudioType.Music, _audioSelectionParameters.SelectedMusicSubType); // TODO: SOUND
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

            FinishCountdown();
        }

        private void FinishCountdown()
        {
            _countdownSequence.AppendCallback(PublishCountdownFinished);
            _countdownSequence.AppendCallback(() => _eventService?.PublishInputEnabled(true));
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
            
            _audioService.PlayUiAudio(EAudioType.Ui, EAudioSubType.Ui_Win); // TODO: SOUND
            
            View.Finish.gameObject.SetActive(true);
            ShowResultImage(View.Finish, View.WinFadeInSeconds);
            
            _eventService.PublishInputEnabled(false);
            _eventService.PublishWinResultChanged(true);
            
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

        private static void SetImageAlpha(Image finish, float alpha)
        {
            if (finish == null)
                return;

            var color = finish.color;
            color.a = alpha;
            finish.color = color;
        }

        private void PublishCountdownFinished()
        {
            if (_eventService == null)
                return;

            _eventService.PublishCountdownFinished();
        }

        private void ShowResultImage(Image finish, float fadeInSeconds)
        {
            if (finish == null)
                return;

            finish.gameObject.SetActive(true);
            var fadeIn = Mathf.Max(0f, fadeInSeconds);
            if (fadeIn <= 0f)
            {
                SetImageAlpha(finish, 1f);
                return;
            }

            SetImageAlpha(finish, 0f);
            _winTween = finish.DOFade(1f, fadeIn).SetLink(finish.gameObject);
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