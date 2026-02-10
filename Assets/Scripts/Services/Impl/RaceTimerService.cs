using System;
using System.Collections.Generic;
using Data.Struct;
using UniRx;
using UnityEngine;

namespace Services.Impl
{
    public class RaceTimerService : IRaceTimerService, IDisposable
    {
        private readonly ILoadingService _loadingService;
        private readonly Subject<RaceLapRecord> _lapCompletedSubject = new();
        private readonly Subject<Unit> _raceFinishedSubject = new();
        private readonly List<RaceLapRecord> _laps = new();
        private IDisposable _startRaceDisposable;

        private float _raceStartTime;
        private float _lastLapStartTime;
        private float _totalRaceTime;
        private int _lastLapIndex;
        private bool _isRunning;
        private bool _isFinished;

        public RaceTimerService(ILoadingService loadingService)
        {
            _loadingService = loadingService;
            _startRaceDisposable = _loadingService.CountdownFinishedStream.Subscribe(_ => StartRace());
        }

        public IObservable<RaceLapRecord> LapCompletedStream => _lapCompletedSubject;
        public IObservable<Unit> RaceFinishedStream => _raceFinishedSubject;
        public IReadOnlyList<RaceLapRecord> Laps => _laps;
        public bool IsRunning => _isRunning;
        public bool IsFinished => _isFinished;
        public float CurrentRaceTime => _isRunning ? Time.time - _raceStartTime : _totalRaceTime;
        public float TotalRaceTime => _totalRaceTime;

        public void StartRace()
        {
            ResetRaceInternal();
            _isRunning = true;
            _raceStartTime = Time.time;
            _lastLapStartTime = _raceStartTime;
        }

        public bool RegisterLap(int lapIndex, bool isFinish)
        {
            if (!_isRunning || _isFinished)
                return false;

            if (lapIndex != _lastLapIndex + 1)
                return false;

            var now = Time.time;
            var lapTime = now - _lastLapStartTime;
            var total = now - _raceStartTime;
            var record = new RaceLapRecord(lapIndex, lapTime, total);
            _laps.Add(record);
            _lapCompletedSubject.OnNext(record);

            _lastLapIndex = lapIndex;
            _lastLapStartTime = now;

            if (isFinish)
            {
                _isFinished = true;
                _isRunning = false;
                _totalRaceTime = total;
                _raceFinishedSubject.OnNext(Unit.Default);
            }

            return true;
        }

        public void ResetRace()
        {
            ResetRaceInternal();
        }

        private void ResetRaceInternal()
        {
            _laps.Clear();
            _lastLapIndex = 0;
            _isRunning = false;
            _isFinished = false;
            _totalRaceTime = 0f;
            _raceStartTime = 0f;
            _lastLapStartTime = 0f;
        }

        public void Dispose()
        {
            _startRaceDisposable?.Dispose();
            _lapCompletedSubject?.OnCompleted();
            _raceFinishedSubject?.OnCompleted();
            _lapCompletedSubject?.Dispose();
            _raceFinishedSubject?.Dispose();
        }
    }
}
