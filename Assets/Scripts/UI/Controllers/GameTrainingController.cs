using System;
using System.Collections.Generic;
using Configs.Impl;
using Data.Enums;
using Data.HelperClass;
using Data.Struct;
using KoboldUi.Element.Controller;
using Services;
using UI.Views;
using UniRx;
using UnityEngine;

namespace UI.Controllers
{
    public class GameTrainingController : AUiController<GameTrainingView>
    {
        private readonly IRaceTimerService _raceTimerService;
        private readonly ILoadingService _loadingService;
        
        private readonly MapSelectionParameters _mapSelectionParameters;
        private readonly GameModeSelectionParameters _gameModeSelectionParameters;
        private readonly TrainingTimeScoreParameters _trainingTimeScoreParameters;
        
        private readonly List<float> _currentSegmentTimes = new();
        private TrainingTimeScoreSetup _currentSetup;
        private int _segmentCount;

        public GameTrainingController(
            IRaceTimerService raceTimerService,
            MapSelectionParameters mapSelectionParameters,
            GameModeSelectionParameters gameModeSelectionParameters,
            TrainingTimeScoreParameters trainingTimeScoreParameters,
            ILoadingService loadingService
        )
        {
            _raceTimerService = raceTimerService;
            _mapSelectionParameters = mapSelectionParameters;
            _gameModeSelectionParameters = gameModeSelectionParameters;
            _trainingTimeScoreParameters = trainingTimeScoreParameters;
            _loadingService = loadingService;
        }

        public override void Initialize()
        {
            if (_raceTimerService == null)
                return;

            _raceTimerService.LapCompletedStream.Subscribe(OnSegmentCompleted).AddTo(View);
            _raceTimerService.RaceFinishedStream.Subscribe(_ => OnRaceFinished()).AddTo(View);
        }

        protected override void OnOpen()
        {
            if (_gameModeSelectionParameters.GameMod == EGameMod.Story) 
                View.gameObject.SetActive(false);

            if (_loadingService.IsTimersRefreshed.Value)
                return;

            InitializeSegmentCount();
            InitializeEntry();
            InitializeDifferenceTexts();
            RefreshBestTime();
            
            _loadingService.PublishTimersRefreshed(true);
        }

        private void InitializeSegmentCount()
        {
            var selectionCount = _mapSelectionParameters != null
                ? Mathf.Max(1, _mapSelectionParameters.SelectedMapSelectionCount)
                : 1;
            var lapCount = _mapSelectionParameters != null
                ? Mathf.Max(1, _mapSelectionParameters.SelectedMapLapCount)
                : 1;

            _segmentCount = selectionCount * lapCount;
            if (_segmentCount < 1)
                _segmentCount = 1;

            _currentSegmentTimes.Clear();
            EnsureSegmentTimeCache();
        }

        private void InitializeEntry()
        {
            if (_trainingTimeScoreParameters == null)
                return;

            var mapId = _mapSelectionParameters != null
                ? _mapSelectionParameters.SelectedMap
                : EMap.None;

            if (mapId == EMap.None)
            {
                _currentSetup = null;
                return;
            }

            _currentSetup = _trainingTimeScoreParameters.GetOrCreateEntry(mapId, _segmentCount);
            _trainingTimeScoreParameters.EnsureSegmentCount(_currentSetup, _segmentCount);
        }

        private void InitializeDifferenceTexts()
        {
            if (View.DifferenceTextList == null)
                return;

            for (var i = 0; i < View.DifferenceTextList.Count; i++)
            {
                var text = View.DifferenceTextList[i];
                if (text == null)
                    continue;

                var isActive = i < _segmentCount;
                text.gameObject.SetActive(isActive);
                if (isActive)
                    text.text = FormatTime(0f);
            }
        }

        private void RefreshBestTime()
        {
            if (View.BestTimeText == null)
                return;

            var bestTime = _currentSetup != null ? _currentSetup.BestTotalTime : 0f;
            View.BestTimeText.text = FormatTime(bestTime);
        }

        private void OnSegmentCompleted(RaceLapRecord record)
        {
            if (View.DifferenceTextList == null)
                return;

            var index = record.LapIndex - 1;
            if (index < 0 || index >= _segmentCount || index >= View.DifferenceTextList.Count)
                return;

            _currentSegmentTimes[index] = record.LapTime;
            var targetText = View.DifferenceTextList[index];
            if (targetText == null)
                return;

            if (_currentSetup == null || _currentSetup.BestSegmentTimes.Count <= index)
            {
                targetText.text = FormatTime(record.LapTime);
                return;
            }

            var bestSegmentTime = _currentSetup.BestSegmentTimes[index];
            if (bestSegmentTime <= 0f)
            {
                targetText.text = FormatTime(record.LapTime);
                return;
            }

            var diff = record.LapTime - bestSegmentTime;
            targetText.text = FormatDifference(diff);
        }

        private void OnRaceFinished()
        {
            if (_raceTimerService == null || _trainingTimeScoreParameters == null)
                return;

            if (_currentSetup == null)
                InitializeEntry();

            if (_currentSetup == null)
                return;

            var totalTime = _raceTimerService.TotalRaceTime;
            var isBest = _currentSetup.BestTotalTime <= 0f || totalTime < _currentSetup.BestTotalTime;
            if (!isBest)
                return;

            _currentSetup.BestTotalTime = totalTime;
            _currentSetup.BestSegmentTimes.Clear();
            for (var i = 0; i < _segmentCount; i++)
                _currentSetup.BestSegmentTimes.Add(_currentSegmentTimes[i]);

            RefreshBestTime();
        }

        private void EnsureSegmentTimeCache()
        {
            while (_currentSegmentTimes.Count < _segmentCount)
                _currentSegmentTimes.Add(0f);
        }

        private string FormatDifference(float diff)
        {
            var sign = diff < 0f ? "-" : "+";
            return $"{sign} {FormatTime(Mathf.Abs(diff))}";
        }

        private string FormatTime(float seconds)
        {
            var clamped = Mathf.Max(0f, seconds);
            var timeSpan = TimeSpan.FromSeconds(clamped);
            var centiseconds = timeSpan.Milliseconds / 10;
            return $"{timeSpan.Minutes:00}''{timeSpan.Seconds:00}''{centiseconds:00}";
        }
    }
}