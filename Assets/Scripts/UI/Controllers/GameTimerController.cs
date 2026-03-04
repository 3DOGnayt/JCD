using System;
using Configs.Impl;
using Data.Struct;
using KoboldUi.Element.Controller;
using Services;
using UI.Views;
using UniRx;
using UnityEngine;

namespace UI.Controllers
{
    public class GameTimerController : AUiController<GameTimerView>
    {
        private readonly IRaceTimerService _raceTimerService;
        private readonly MapSelectionParameters _mapSelectionParameters;
        private IDisposable _totalTimeDisposable;

        public GameTimerController(
            IRaceTimerService raceTimerService,
            MapSelectionParameters mapSelectionParameters)
        {
            _raceTimerService = raceTimerService;
            _mapSelectionParameters = mapSelectionParameters;
        }

        public override void Initialize()
        {
            if (_raceTimerService == null)
                return;

            _raceTimerService.LapCompletedStream.Subscribe(UpdateLapTime).AddTo(View);
        }

        protected override void OnOpen()
        {
            InitializeSelectionTimes();
            StartTotalTimeUpdates();
        }

        private void InitializeSelectionTimes()
        {
            if (View.SelectionTimeTextList == null)
                return;

            var configuredSelectionCount = _mapSelectionParameters != null
                ? _mapSelectionParameters.SelectedMapSelectionCount
                : 0;
            var configuredLapCount = _mapSelectionParameters != null
                ? _mapSelectionParameters.SelectedMapLapCount
                : 0;
            var useSelectionCount = View.SelectionTimeTextList.Count;
            if (configuredSelectionCount > 1)
                useSelectionCount = configuredSelectionCount;
            else if (configuredLapCount > 0)
                useSelectionCount = configuredLapCount;
            else if (configuredSelectionCount > 0)
                useSelectionCount = configuredSelectionCount;

            for (var i = 0; i < View.SelectionTimeTextList.Count; i++)
            {
                var text = View.SelectionTimeTextList[i];
                if (text == null)
                    continue;

                var isActive = i < useSelectionCount;
                text.gameObject.SetActive(isActive);
                if (isActive)
                    text.text = FormatTime(0f);
            }

            if (_raceTimerService == null)
                return;

            foreach (var record in _raceTimerService.Laps)
                UpdateLapTime(record);
        }

        private void StartTotalTimeUpdates()
        {
            if (View.TotalTimeText == null || _raceTimerService == null)
                return;

            _totalTimeDisposable?.Dispose();
            _totalTimeDisposable = Observable.EveryUpdate()
                .Subscribe(_ => View.TotalTimeText.text = FormatTime(_raceTimerService.CurrentRaceTime));
            _totalTimeDisposable.AddTo(View);
        }

        private void UpdateLapTime(RaceLapRecord record)
        {
            if (View.SelectionTimeTextList == null)
                return;

            var index = record.LapIndex - 1;
            if (index < 0 || index >= View.SelectionTimeTextList.Count)
                return;

            var text = View.SelectionTimeTextList[index];
            if (text == null)
                return;

            text.text = FormatTime(record.LapTime);
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