using Components;
using Scellecs.Morpeh;
using Services;
using UnityEngine;
using Zenject;

namespace Systems.Race
{
    public class RaceLapSystem : ISystem
    {
        [Inject] public World World { get; set; }

        private IUnitRaceTimerService _unitRaceTimerService;
        private Filter _configFilter;
        private Filter _eventFilter;

        private Stash<RaceLapConfigComponent> _configStash;
        private Stash<RaceLapStateComponent> _stateStash;
        private Stash<RaceLapTriggerEventComponent> _eventStash;
        private Stash<PlayerTagComponent> _playerTagStash;
        private Stash<OpponentTagComponent> _opponentTagStash;

        [Inject]
        public void Construct(IUnitRaceTimerService unitRaceTimerService)
        {
            _unitRaceTimerService = unitRaceTimerService;
        }

        public void OnAwake()
        {
            _configFilter = World.Filter
                .With<RaceLapConfigComponent>()
                .With<RaceLapStateComponent>()
                .Build();
            
            _eventFilter = World.Filter
                .With<RaceLapTriggerEventComponent>()
                .Build();

            _configStash = World.GetStash<RaceLapConfigComponent>();
            _stateStash = World.GetStash<RaceLapStateComponent>();
            _eventStash = World.GetStash<RaceLapTriggerEventComponent>();
            _playerTagStash = World.GetStash<PlayerTagComponent>();
            _opponentTagStash = World.GetStash<OpponentTagComponent>();
        }

        public void OnUpdate(float deltaTime)
        {
            if (!TryGetConfigEntity(out var configEntity))
            {
                DisposeEvents();
                return;
            }

            if (!IsRaceActive())
            {
                DisposeEvents();
                return;
            }

            ProcessEvents(configEntity);
        }

        public void Dispose() { }

        private bool TryGetConfigEntity(out Entity configEntity)
        {
            foreach (var entity in _configFilter)
            {
                configEntity = entity;
                return true;
            }

            configEntity = default;
            return false;
        }

        private void DisposeEvents()
        {
            foreach (var eventEntity in _eventFilter)
                World.RemoveEntity(eventEntity);
        }

        private bool IsRaceActive()
        {
            return _unitRaceTimerService != null && _unitRaceTimerService.IsRaceActive;
        }

        private void ProcessEvents(Entity configEntity)
        {
            ref var config = ref _configStash.Get(configEntity);

            var totalSelections = Mathf.Max(1, config.SelectionCount);
            var totalLaps = Mathf.Max(1, config.LapCount);
            var isLoopMap = config.CheckpointsCount <= 1;

            foreach (var eventEntity in _eventFilter)
            {
                var evt = _eventStash.Get(eventEntity);
                var carEntity = evt.CarEntity;
                if (!World.Has(carEntity))
                {
                    World.RemoveEntity(eventEntity);
                    continue;
                }

                if (!_stateStash.Has(carEntity))
                    _stateStash.Set(carEntity, new RaceLapStateComponent());

                ref var state = ref _stateStash.Get(carEntity);
                var isPlayer = _playerTagStash.Has(carEntity);
                var isOpponent = _opponentTagStash.Has(carEntity);
                if (!isPlayer && !isOpponent)
                {
                    World.RemoveEntity(eventEntity);
                    continue;
                }

                if (isPlayer)
                    _unitRaceTimerService.SetPlayerEntity(carEntity);

                if (isLoopMap)
                    ProcessLoopEvent(ref state, totalLaps, carEntity, eventEntity);
                else
                    ProcessCheckpointEvent(evt, ref state, totalSelections, carEntity, eventEntity);
            }
        }

        private void ProcessLoopEvent(
            ref RaceLapStateComponent state,
            int totalLaps,
            Entity carEntity,
            Entity eventEntity)
        {
            if (!state.IsLoopStarted)
            {
                state.IsLoopStarted = true;
                World.RemoveEntity(eventEntity);
                return;
            }

            state.CurrentLap++;
            var isFinish = state.CurrentLap >= totalLaps;
            var isLoopRegistered = _unitRaceTimerService.RegisterLap(carEntity, state.CurrentLap, isFinish);
            if (isFinish && isLoopRegistered && _playerTagStash.Has(carEntity))
                Debug.Log("Race finished");

            World.RemoveEntity(eventEntity);
        }

        private void ProcessCheckpointEvent(
            RaceLapTriggerEventComponent evt,
            ref RaceLapStateComponent state,
            int totalSelections,
            Entity carEntity,
            Entity eventEntity)
        {
            if (evt.CheckpointIndex != state.NextCheckpointIndex)
            {
                World.RemoveEntity(eventEntity);
                return;
            }

            state.NextCheckpointIndex++;
            state.CurrentSelection++;

            var isSegmentFinish = state.CurrentSelection >= totalSelections;
            var isRegistered = _unitRaceTimerService.RegisterLap(carEntity, state.CurrentSelection, isSegmentFinish);
            if (isSegmentFinish && isRegistered && _playerTagStash.Has(carEntity))
                Debug.Log("Race finished");

            World.RemoveEntity(eventEntity);
        }
    }
}