using System;
using System.Collections.Generic;
using Data.Struct;
using Scellecs.Morpeh;
using UniRx;
using UnityEngine;

namespace Services.Impl
{
    public class RaceTimerService : IRaceTimerService, IUnitRaceTimerService, IDisposable
    {
        private readonly IEventService _eventService;
        private readonly Subject<RaceLapRecordEntry> _lapCompletedSubject = new();
        private readonly Subject<Unit> _raceFinishedSubject = new();
        private readonly Subject<Entity> _unitRaceFinishedSubject = new();
        private readonly Dictionary<Entity, RaceState> _states = new();
        private IDisposable _startRaceDisposable;

        private float _raceStartTime;
        private bool _raceActive;
        private Entity _playerEntity;
        private bool _hasPlayerEntity;

        private sealed class RaceState
        {
            public readonly List<RaceLapRecordEntry> Laps = new();
            public float RaceStartTime;
            public float LastLapStartTime;
            public float TotalRaceTime;
            public int LastLapIndex;
            public bool IsRunning;
            public bool IsFinished;
        }

        public RaceTimerService(IEventService eventService)
        {
            _eventService = eventService;
            if (_eventService != null)
                _startRaceDisposable = _eventService.CountdownFinishedStream.Subscribe(_ => StartRace());
        }

        public IObservable<RaceLapRecordEntry> LapCompletedStream => _lapCompletedSubject;
        public IObservable<Unit> RaceFinishedStream => _raceFinishedSubject;
        public IReadOnlyList<RaceLapRecordEntry> Laps => TryGetPlayerState(out var state) ? state.Laps : Array.Empty<RaceLapRecordEntry>();
        public bool IsRunning => TryGetPlayerState(out var state) && state.IsRunning;
        public bool RaceIsFinished => TryGetPlayerState(out var state) && state.IsFinished;
        public float CurrentRaceTime => TryGetPlayerState(out var state) && state.IsRunning ? Time.time - state.RaceStartTime : TryGetPlayerTotalTime();
        public float CurrentLapTime => TryGetPlayerState(out var state) && state.IsRunning ? Time.time - state.LastLapStartTime : 0f;
        public float TotalRaceTime => TryGetPlayerState(out var state) ? state.TotalRaceTime : 0f;
        public IObservable<Entity> RaceFinishedEntityStream => _unitRaceFinishedSubject;
        public bool IsRaceActive => _raceActive;

        public void StartRace()
        {
            ResetRaceInternal();
            _raceActive = true;
            _raceStartTime = Time.time;

            if (_hasPlayerEntity)
                CreateState(_playerEntity, _raceStartTime);
        }

        public void ResetRace() => ResetRaceInternal();

        public bool IsFinished(Entity entity) => TryGetState(entity, out var state) && state.IsFinished;

        public void SetPlayerEntity(Entity entity)
        {
            _playerEntity = entity;
            _hasPlayerEntity = true;

            if (_raceActive && !TryGetState(entity, out _))
                CreateState(entity, _raceStartTime);
        }

        public bool RegisterLap(Entity entity, int lapIndex, bool isFinish)
        {
            if (!_raceActive)
                return false;

            var state = GetOrCreateState(entity);
            if (!state.IsRunning || state.IsFinished)
                return false;

            if (lapIndex != state.LastLapIndex + 1)
                return false;

            var now = Time.time;
            var lapTime = now - state.LastLapStartTime;
            var total = now - state.RaceStartTime;
            var record = new RaceLapRecordEntry(lapIndex, lapTime, total);
            
            state.Laps.Add(record);
            
            if (_hasPlayerEntity && entity.Equals(_playerEntity))
                _lapCompletedSubject.OnNext(record);

            state.LastLapIndex = lapIndex;
            state.LastLapStartTime = now;

            if (isFinish)
            {
                state.IsFinished = true;
                state.IsRunning = false;
                state.TotalRaceTime = total;
                _unitRaceFinishedSubject.OnNext(entity);

                if (_hasPlayerEntity && entity.Equals(_playerEntity))
                    _raceFinishedSubject.OnNext(Unit.Default);
            }

            return true;
        }

        private void ResetRaceInternal()
        {
            _states.Clear();
            _raceActive = false;
            _raceStartTime = 0f;
        }

        private RaceState GetOrCreateState(Entity entity)
        {
            if (TryGetState(entity, out var existing))
                return existing;

            return CreateState(entity, _raceStartTime);
        }

        private RaceState CreateState(Entity entity, float startTime)
        {
            var state = new RaceState
            {
                RaceStartTime = startTime,
                LastLapStartTime = startTime,
                TotalRaceTime = 0f,
                LastLapIndex = 0,
                IsRunning = true,
                IsFinished = false
            };
            _states[entity] = state;
            return state;
        }

        private bool TryGetState(Entity entity, out RaceState state)
        {
            return _states.TryGetValue(entity, out state);
        }

        private bool TryGetPlayerState(out RaceState state)
        {
            state = null;
            if (!_hasPlayerEntity)
                return false;

            if (TryGetState(_playerEntity, out state))
                return true;

            if (!_raceActive)
                return false;

            state = CreateState(_playerEntity, _raceStartTime);
            return true;
        }

        private float TryGetPlayerTotalTime()
        {
            if (!TryGetPlayerState(out var state))
                return 0f;

            return state.TotalRaceTime;
        }

        public void Dispose()
        {
            _startRaceDisposable?.Dispose();
            _lapCompletedSubject?.OnCompleted();
            _raceFinishedSubject?.OnCompleted();
            _unitRaceFinishedSubject?.OnCompleted();
            _lapCompletedSubject?.Dispose();
            _raceFinishedSubject?.Dispose();
            _unitRaceFinishedSubject?.Dispose();
        }
    }
}