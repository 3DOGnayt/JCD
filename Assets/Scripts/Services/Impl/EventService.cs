using System;
using Components;
using UniRx;
using Cameras;
using Helpers.Car;

namespace Services.Impl
{
    public class EventService : IEventService, IDisposable
    {
        private readonly ReactiveProperty<float> _loadingProgress = new();
        private readonly ReactiveProperty<bool> _isLoadingCompleted = new();
        private readonly ReactiveProperty<bool> _isGameStarted = new();
        private readonly ReactiveProperty<bool> _isTimersRefreshed = new();
        
        private readonly Subject<Unit> _startRaceSubject = new();
        private readonly Subject<Unit> _countdownBeforeStartFinishedSubject = new();
        private readonly Subject<ICarView> _playerSpawnedSubject = new();
        private readonly Subject<MinimapCameraHolder> _minimapSpawnedSubject = new();
        private readonly Subject<CarSetupAspect> _carSetupChangedSubject = new();
        private readonly Subject<Unit> _carSelectionChangedSubject = new();
        private readonly Subject<bool> _inputEnabledSubject = new();
        private readonly Subject<bool> _resultSubject = new();

        public IReactiveProperty<float> LoadingProgress => _loadingProgress;
        public IReactiveProperty<bool> IsLoadingCompleted => _isLoadingCompleted;
        public IReactiveProperty<bool> IsGameStarted => _isGameStarted;
        public IReactiveProperty<bool> IsTimersRefreshed => _isTimersRefreshed;

        
        public IObservable<Unit> StartRaceStream => _startRaceSubject;
        public IObservable<Unit> CountdownFinishedStream => _countdownBeforeStartFinishedSubject;
        public IObservable<ICarView> PlayerSpawnedStream => _playerSpawnedSubject;
        public IObservable<MinimapCameraHolder> MinimapSpawnedStream => _minimapSpawnedSubject;
        public IObservable<CarSetupAspect> CarSetupChangedStream => _carSetupChangedSubject;
        public IObservable<Unit> CarSelectionChangedStream => _carSelectionChangedSubject;
        public IObservable<bool> InputEnabledStream => _inputEnabledSubject;
        public IObservable<bool> ResultSubject => _resultSubject;

        public void PublishStartRace() => _startRaceSubject.OnNext(Unit.Default);
        public void PublishCountdownFinished() => _countdownBeforeStartFinishedSubject.OnNext(Unit.Default);

        public void PublishPlayerSpawned(ICarView carView) => _playerSpawnedSubject.OnNext(carView);
        public void PublishMinimapSpawned(MinimapCameraHolder minimapCamera) => _minimapSpawnedSubject.OnNext(minimapCamera);

        public void PublishCarSetupChanged(CarSetupAspect carSetupAspect) => _carSetupChangedSubject.OnNext(carSetupAspect);
        public void PublishCarSelectionChanged() => _carSelectionChangedSubject.OnNext(Unit.Default);
        public void PublishInputEnabled(bool isEnabled) => _inputEnabledSubject.OnNext(isEnabled);
        public void PublishWinResultChanged(bool isEnabled) => _resultSubject.OnNext(isEnabled);
        public void PublishGameStarted(bool isEnabled) => _isGameStarted.Value = isEnabled;
        public void PublishTimersRefreshed(bool isEnabled) => _isTimersRefreshed.Value = isEnabled;

        public void ResetEvents()
        {
            _isGameStarted.Value = false;
            _isTimersRefreshed.Value = false;
        }

        public void Dispose()
        {
            _startRaceSubject?.OnCompleted();
            _countdownBeforeStartFinishedSubject?.OnCompleted();
            _playerSpawnedSubject?.OnCompleted();
            _minimapSpawnedSubject?.OnCompleted();
            _carSetupChangedSubject?.OnCompleted();
            _carSelectionChangedSubject?.OnCompleted();
            _inputEnabledSubject?.OnCompleted();

            _startRaceSubject?.Dispose();
            _countdownBeforeStartFinishedSubject?.Dispose();
            _playerSpawnedSubject?.Dispose();
            _minimapSpawnedSubject?.Dispose();
            _carSetupChangedSubject?.Dispose();
            _carSelectionChangedSubject?.Dispose();
            _inputEnabledSubject?.Dispose();
        }
    }
}