using System;
using System.Collections.Generic;
using Configs.Impl;
using Data.Enums;
using DG.Tweening;
using KoboldUi.Element.Controller;
using Services;
using UI.Views;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Controllers
{
    public class GameResultController : AUiController<GameResultView>, IDisposable
    {
        private readonly IEventService _eventService;
        private readonly IRaceTimerService _raceTimerService;
        private readonly IGameSessionService _gameSessionService;
        private readonly IAudioService _audioService;
        private readonly GameModeSelectionParameters _gameModeSelectionParameters;
        
        private IDisposable _resultSubscription;
        private Sequence _winMoveSequence;
        private readonly Dictionary<Image, Vector2> _resultStartPositions = new();

        private bool _resultGame;

        public GameResultController(
            IEventService eventService,
            IRaceTimerService raceTimerService,
            IGameSessionService gameSessionService,
            IAudioService audioService,
            GameModeSelectionParameters gameModeSelectionParameters
        )
        {
            _eventService = eventService;
            _raceTimerService = raceTimerService;
            _gameSessionService = gameSessionService;
            _audioService = audioService;
            _gameModeSelectionParameters = gameModeSelectionParameters;
        }

        public override void Initialize()
        {
            View.Win.gameObject.SetActive(false);
            View.Lose.gameObject.SetActive(false);
            View.Complite.gameObject.SetActive(false);
            View.TimePanel.SetActive(false);
            View.ResultButtons.SetActive(false);
            
            View.Retry.OnClickAsObservable().Subscribe(_ => OnRetryClick()).AddTo(View);
            View.MainMenu.OnClickAsObservable().Subscribe(_ => OnMainMenuClick()).AddTo(View);
            
            _resultSubscription = _eventService.ResultSubject.Subscribe(SetResult);
        }

        protected override void OnOpen()
        {
            _eventService?.PublishInputEnabled(false);
            
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
            _audioService.PlayUiAudio(EAudioType.Ui, EAudioSubType.Ui_Win);

            var gameMod = _gameModeSelectionParameters != null ? _gameModeSelectionParameters.GameMod : EGameMod.None;
            if (gameMod == EGameMod.Training)
            {
                View.Complite.gameObject.SetActive(true);
                View.Win.gameObject.SetActive(false);
                View.Lose.gameObject.SetActive(false);
                return;
            }

            View.Complite.gameObject.SetActive(false);

            if (gameMod == EGameMod.Story)
            {
                View.Win.gameObject.SetActive(true);
                View.Lose.gameObject.SetActive(false);
                return;
            }

            View.Win.gameObject.SetActive(resultGame);
            View.Lose.gameObject.SetActive(!resultGame);
        }

        private void PlayWinMoveSequence()
        {
            var targetImage = GetResultImage();
            if (targetImage == null)
                return;

            var rect = targetImage.rectTransform;
            var startPosition = GetStartPosition(targetImage);
            rect.anchoredPosition = startPosition;

            _winMoveSequence?.Kill();
            var target = startPosition + Vector2.up * View.WinMoveUpDistance;
           
            _winMoveSequence = DOTween.Sequence()
                .SetLink(targetImage.gameObject)
                .AppendInterval(View.DelayBeforeMoveResult)
                .Append(rect.DOAnchorPos(target, View.WinMoveUpDuration).SetEase(View.MoveEase))
                .AppendInterval(View.DelayBeforeShowResults)
                .OnComplete(ShowResults);
        }

        private Image GetResultImage()
        {
            if (View.Complite.gameObject.activeSelf)
                return View.Complite;

            if (View.Win.gameObject.activeSelf)
                return View.Win;

            if (View.Lose.gameObject.activeSelf)
                return View.Lose;

            return null;
        }

        private Vector2 GetStartPosition(Image image)
        {
            if (image == null)
                return Vector2.zero;

            if (_resultStartPositions.TryGetValue(image, out var cached))
                return cached;

            var pos = image.rectTransform.anchoredPosition;
            _resultStartPositions[image] = pos;
            return pos;
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
            _audioService.PlayUiAudio(EAudioType.Ui, EAudioSubType.MenuButtonSelect);
            
            _gameSessionService?.RestartGame();
        }

        private void OnMainMenuClick()
        {
            _audioService.PlayUiAudio(EAudioType.Ui, EAudioSubType.MenuButtonBack);
            
            _gameSessionService?.ExitToMenu();
        }

        public void Dispose()
        {
            _resultSubscription?.Dispose();
            _winMoveSequence?.Kill();
        }
    }
}