using System;
using System.Collections.Generic;
using Configs.Impl;
using Data.Enums;
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
        private readonly GameSelectionParameters _gameSelectionParameters;
        private readonly TrainingTimeScoreParameters _trainingTimeScoreParameters;
        private readonly List<float> _currentSegmentTimes = new List<float>();
        private TrainingTimeScoreEntry _currentEntry;
        private int _segmentCount;

        public GameTrainingController(
            IRaceTimerService raceTimerService,
            GameSelectionParameters gameSelectionParameters,
            TrainingTimeScoreParameters trainingTimeScoreParameters)
        {
            _raceTimerService = raceTimerService;
            _gameSelectionParameters = gameSelectionParameters;
            _trainingTimeScoreParameters = trainingTimeScoreParameters;
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
            if (_gameSelectionParameters.GameMod == EGameMod.Story) 
                View.gameObject.SetActive(false);
            
            InitializeSegmentCount();
            InitializeEntry();
            InitializeDifferenceTexts();
            RefreshBestTime();
        }

        private void InitializeSegmentCount()
        {
            var selectionCount = _gameSelectionParameters != null
                ? Mathf.Max(1, _gameSelectionParameters.SelectedMapSelectionCount)
                : 1;
            var lapCount = _gameSelectionParameters != null
                ? Mathf.Max(1, _gameSelectionParameters.SelectedMapLapCount)
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

            var mapId = _gameSelectionParameters != null
                ? _gameSelectionParameters.SelectedMap
                : EMap.None;

            if (mapId == EMap.None)
            {
                _currentEntry = null;
                return;
            }

            _currentEntry = _trainingTimeScoreParameters.GetOrCreateEntry(mapId, _segmentCount);
            _trainingTimeScoreParameters.EnsureSegmentCount(_currentEntry, _segmentCount);
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

            var bestTime = _currentEntry != null ? _currentEntry.BestTotalTime : 0f;
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

            if (_currentEntry == null || _currentEntry.BestSegmentTimes.Count <= index)
            {
                targetText.text = FormatTime(record.LapTime);
                return;
            }

            var bestSegmentTime = _currentEntry.BestSegmentTimes[index];
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

            if (_currentEntry == null)
                InitializeEntry();

            if (_currentEntry == null)
                return;

            var totalTime = _raceTimerService.TotalRaceTime;
            var isBest = _currentEntry.BestTotalTime <= 0f || totalTime < _currentEntry.BestTotalTime;
            if (!isBest)
                return;

            _currentEntry.BestTotalTime = totalTime;
            _currentEntry.BestSegmentTimes.Clear();
            for (var i = 0; i < _segmentCount; i++)
                _currentEntry.BestSegmentTimes.Add(_currentSegmentTimes[i]);

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
