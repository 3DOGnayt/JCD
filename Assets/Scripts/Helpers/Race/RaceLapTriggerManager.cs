using System;
using System.Collections.Generic;
using Components;
using Configs.Impl;
using Scellecs.Morpeh;
using Services;
using UniRx;
using UnityEngine;
using Zenject;

namespace Helpers.Race
{
    public class RaceLapTriggerManager : MonoBehaviour
    {
        [SerializeField] private List<RaceLapTrigger> _checkpoints = new();
        
        private World _world;
        private IEventService _eventService;
        private MapSelectionParameters _mapSelectionParameters;
        
        private Stash<RaceLapStateComponent> _stateStash;
        private IDisposable _startRaceDisposable;
        
        private Entity _raceLapEntity;
        private bool _hasEntity;

        [Inject]
        public void Construct(
            IEventService eventService,
            MapSelectionParameters mapSelectionParameters,
            World world
        )
        {
            _eventService = eventService;
            _mapSelectionParameters = mapSelectionParameters;
            _world = world;
            
            if (_eventService != null)
                _startRaceDisposable = _eventService.StartRaceStream.Subscribe(_ => ResetProgress());
        }

        private void Awake()
        {
            for (var i = 0; i < _checkpoints.Count; i++)
            {
                var checkpoint = _checkpoints[i];
                if (checkpoint == null)
                    continue;

                checkpoint.Configure(i);
            }

            InitializeRaceLapEntity();
        }

        private void InitializeRaceLapEntity()
        {
            if (_world == null)
                return;

            if (_hasEntity)
                return;

            _stateStash = _world.GetStash<RaceLapStateComponent>();

            _raceLapEntity = _world.CreateEntity();
            _raceLapEntity.SetComponent(BuildConfig());
            _raceLapEntity.SetComponent(new RaceLapStateComponent());
            _hasEntity = true;
        }

        private void ResetProgress()
        {
            if (!_hasEntity || _stateStash == null)
                return;

            ref var state = ref _stateStash.Get(_raceLapEntity);
            state.NextCheckpointIndex = 0;
            state.CurrentSelection = 0;
            state.CurrentLap = 0;
            state.IsLoopStarted = false;
        }

        private RaceLapConfigComponent BuildConfig()
        {
            var selectionCount = 1;
            var lapCount = 1;

            if (_mapSelectionParameters != null)
            {
                if (_mapSelectionParameters.SelectedMapSelectionCount > 0)
                    selectionCount = _mapSelectionParameters.SelectedMapSelectionCount;
                if (_mapSelectionParameters.SelectedMapLapCount > 0)
                    lapCount = _mapSelectionParameters.SelectedMapLapCount;
            }

            return new RaceLapConfigComponent
            {
                SelectionCount = selectionCount,
                LapCount = lapCount,
                CheckpointsCount = _checkpoints.Count
            };
        }

        private void OnDestroy()
        {
            _startRaceDisposable?.Dispose();
            
            if (_world != null && _hasEntity)
                _world.RemoveEntity(_raceLapEntity);
        }
    }
}