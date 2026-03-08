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

        private IRaceTimerService _raceTimerService;
        private Filter _configFilter;
        private Filter _eventFilter;

        private Stash<RaceLapConfigComponent> _configStash;
        private Stash<RaceLapStateComponent> _stateStash;
        private Stash<RaceLapTriggerEventComponent> _eventStash;

        [Inject]
        public void Construct(IRaceTimerService raceTimerService)
        {
            _raceTimerService = raceTimerService;
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
            return _raceTimerService != null && _raceTimerService.IsRunning && !_raceTimerService.IsFinished;
        }

        private void ProcessEvents(Entity configEntity)
        {
            ref var config = ref _configStash.Get(configEntity);
            ref var state = ref _stateStash.Get(configEntity);

            var totalSelections = Mathf.Max(1, config.SelectionCount);
            var totalLaps = Mathf.Max(1, config.LapCount);
            var isLoopMap = config.CheckpointsCount <= 1;

            foreach (var eventEntity in _eventFilter)
            {
                var evt = _eventStash.Get(eventEntity);
                if (isLoopMap)
                    ProcessLoopEvent(ref state, totalLaps, eventEntity);
                else
                    ProcessCheckpointEvent(evt, ref state, totalSelections, eventEntity);
            }
        }

        private void ProcessLoopEvent(ref RaceLapStateComponent state, int totalLaps, Entity eventEntity)
        {
            if (!state.IsLoopStarted)
            {
                state.IsLoopStarted = true;
                World.RemoveEntity(eventEntity);
                return;
            }

            state.CurrentLap++;
            var isFinish = state.CurrentLap >= totalLaps;
            var isLoopRegistered = _raceTimerService.RegisterLap(state.CurrentLap, isFinish);

            if (isFinish && isLoopRegistered)
                Debug.Log("Race finished");

            World.RemoveEntity(eventEntity);
        }

        private void ProcessCheckpointEvent(
            RaceLapTriggerEventComponent evt,
            ref RaceLapStateComponent state,
            int totalSelections,
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
            var isRegistered = _raceTimerService.RegisterLap(state.CurrentSelection, isSegmentFinish);

            if (isSegmentFinish && isRegistered)
                Debug.Log("Race finished");

            World.RemoveEntity(eventEntity);
        }
    }
}