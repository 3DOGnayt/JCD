using System.Collections.Generic;
using Configs.Impl;
using Services;
using UniRx;
using UnityEngine;
using Zenject;

namespace Helpers.Race
{
    public class RaceLapTriggerManager : MonoBehaviour
    {
        [SerializeField] private List<RaceLapTrigger> _checkpoints = new();
        
        private IRaceTimerService _raceTimerService;
        private MapSelectionParameters _mapSelectionParameters;
        
        private int _selectionCount = 1;
        private int _lapCount = 1;
        
        private int _nextCheckpointIndex;
        private int _currentSelection;
        private int _currentLap;
        private bool _isLoopStarted;

        [Inject]
        public void Construct(
            IRaceTimerService raceTimerService,
            ILoadingService loadingService,
            MapSelectionParameters mapSelectionParameters)
        {
            _raceTimerService = raceTimerService;
            _mapSelectionParameters = mapSelectionParameters;
            loadingService.StartRaceStream.Subscribe(_ => ResetProgress()).AddTo(this);
        }

        private void Awake()
        {
            for (var i = 0; i < _checkpoints.Count; i++)
            {
                var checkpoint = _checkpoints[i];
                if (checkpoint == null)
                    continue;

                checkpoint.Configure(this, i);
            }

            if (_mapSelectionParameters != null)
            {
                if (_mapSelectionParameters.SelectedMapSelectionCount > 0)
                    _selectionCount = _mapSelectionParameters.SelectedMapSelectionCount;
                if (_mapSelectionParameters.SelectedMapLapCount > 0)
                    _lapCount = _mapSelectionParameters.SelectedMapLapCount;
            }
        }

        public void RegisterCheckpoint(int checkpointIndex)
        {
            if (_raceTimerService == null || !_raceTimerService.IsRunning || _raceTimerService.IsFinished)
                return;

            if (_checkpoints.Count == 0)
                return;

            var totalSelections = Mathf.Max(1, _selectionCount);
            var totalLaps = Mathf.Max(1, _lapCount);
            var isLoopMap = _checkpoints.Count <= 1;

            if (isLoopMap)
            {
                if (!_isLoopStarted)
                {
                    _isLoopStarted = true;
                    return;
                }

                _currentLap++;
                var isFinish = _currentLap >= totalLaps;
                var isLoopRegistered = _raceTimerService.RegisterLap(_currentLap, isFinish);
                if (isFinish && isLoopRegistered)
                    Debug.Log("Race finished");
                return;
            }

            if (checkpointIndex != _nextCheckpointIndex)
                return;

            _nextCheckpointIndex++;
            _currentSelection++;

            var isSegmentFinish = _currentSelection >= totalSelections;
            var isRegistered = _raceTimerService.RegisterLap(_currentSelection, isSegmentFinish);
            if (isSegmentFinish && isRegistered)
                Debug.Log("Race finished");
        }

        private void ResetProgress()
        {
            _nextCheckpointIndex = 0;
            _currentSelection = 0;
            _currentLap = 0;
            _isLoopStarted = false;
        }
    }
}