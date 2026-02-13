using System;
using DG.Tweening;
using KoboldUi.Element.Controller;
using Services;
using UI.Views;
using UniRx;
using UnityEngine;

namespace UI.Controllers
{
    public class GameResultController : AUiController<GameResultView>, IDisposable
    {
        private readonly ILoadingService _loadingService;
        private readonly IRaceTimerService _raceTimerService;
        private readonly IGameSessionService _gameSessionService;
        
        private IDisposable _resultSubscription;
        private Sequence _winMoveSequence;
        private Vector2 _winStartAnchoredPosition;
        private bool _hasWinStartPosition;

        private bool _resultGame;

        public GameResultController(
            ILoadingService loadingService,
            IRaceTimerService raceTimerService,
            IGameSessionService gameSessionService
        )
        {
            _loadingService = loadingService;
            _raceTimerService = raceTimerService;
            _gameSessionService = gameSessionService;
        }

        public override void Initialize()
        {
            View.Win.gameObject.SetActive(false);
            View.Lose.gameObject.SetActive(false);
            View.TimePanel.SetActive(false);
            View.ResultButtons.SetActive(false);
            
            View.Retry.OnClickAsObservable().Subscribe(_ => OnRetryClick()).AddTo(View);
            View.MainMenu.OnClickAsObservable().Subscribe(_ => OnMainMenuClick()).AddTo(View);
            
            _resultSubscription = _loadingService.ResultSubject.Subscribe(SetResult);
        }

        protected override void OnOpen()
        {
            _loadingService?.PublishInputEnabled(false);
            
            SetWinResult(_resultGame);
            View.TimePanel.SetActive(false);
            View.ResultButtons.SetActive(false);
            
            PlayWinMoveSequence();
        }

        private void SetResult(bool isActive)
        {
            _resultGame = isActive;
        }

        private void SetWinResult(bool resultGame)
        {
            View.Win.gameObject.SetActive(resultGame);
            View.Lose.gameObject.SetActive(!resultGame);
        }

        private void PlayWinMoveSequence()
        {
            var rect = View.Win.rectTransform;
            if (!_hasWinStartPosition)
            {
                _winStartAnchoredPosition = rect.anchoredPosition;
                _hasWinStartPosition = true;
            }

            rect.anchoredPosition = _winStartAnchoredPosition;

            _winMoveSequence?.Kill();
            var target = _winStartAnchoredPosition + Vector2.up * View.WinMoveUpDistance;
           
            _winMoveSequence = DOTween.Sequence()
                .SetLink(View.Win.gameObject)
                .AppendInterval(View.DelayBeforeMoveResult)
                .Append(rect.DOAnchorPos(target, View.WinMoveUpDuration).SetEase(View.MoveEase))
                .AppendInterval(View.DelayBeforeShowResults)
                .OnComplete(ShowResults);
        }

        private void ShowResults()
        {
            View.TimePanel.SetActive(true);
            View.ResultButtons.SetActive(true);

            FillTimeValues();
        }

        private void FillTimeValues()
        {
            if (_raceTimerService == null || View.SelectionTimeList == null)
                return;

            if (View.TotalTime != null)
                View.TotalTime.text = FormatTime(_raceTimerService.TotalRaceTime);

            var laps = _raceTimerService.Laps;
            for (var i = 0; i < View.SelectionTimeList.Count; i++)
            {
                var text = View.SelectionTimeList[i];
                if (text == null)
                    continue;

                var hasValue = laps != null && i < laps.Count;
                text.gameObject.SetActive(hasValue);
                if (hasValue)
                    text.text = $"<color=orange>{i + 1}</color> {FormatTime(laps[i].LapTime)}";
            }
        }

        private static string FormatTime(float seconds)
        {
            var clamped = Mathf.Max(0f, seconds);
            var timeSpan = TimeSpan.FromSeconds(clamped);
            var centiseconds = timeSpan.Milliseconds / 10;
            return $"{timeSpan.Minutes:00}''{timeSpan.Seconds:00}''{centiseconds:00}";
        }

        private void OnRetryClick()
        {
            _gameSessionService?.RestartGame();
        }

        private void OnMainMenuClick()
        {
            _gameSessionService?.ExitToMenu();
        }

        public void Dispose()
        {
            _resultSubscription?.Dispose();
            _winMoveSequence?.Kill();
        }
    }
}
