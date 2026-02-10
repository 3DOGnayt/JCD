using System;
using Components;
using UniRx;
using Views;

namespace Services.Impl
{
    public class LoadingService : ILoadingService, IDisposable
    {
        private readonly ReactiveProperty<float> _loadingProgress = new();
        private readonly ReactiveProperty<bool> _isLoadingCompleted = new();
        private readonly Subject<Unit> _startRaceSubject = new();
        private readonly Subject<Unit> _countdownBeforeStartFinishedSubject = new();
        private readonly Subject<ICarView> _playerSpawnedSubject = new();
        private readonly Subject<CarSetupAspect> _carSetupChangedSubject = new();
        private readonly Subject<bool> _inputEnabledSubject = new();
        private readonly Subject<bool> _resultSubject = new();
        
        public IReactiveProperty<float> LoadingProgress => _loadingProgress;
        public IReactiveProperty<bool> IsLoadingCompleted => _isLoadingCompleted;
        public IObservable<Unit> StartRaceStream => _startRaceSubject;
        public IObservable<Unit> CountdownFinishedStream => _countdownBeforeStartFinishedSubject;
        public IObservable<ICarView> PlayerSpawnedStream => _playerSpawnedSubject;
        public IObservable<CarSetupAspect> CarSetupChangedStream => _carSetupChangedSubject;
        public IObservable<bool> InputEnabledStream => _inputEnabledSubject;
        public IObservable<bool> ResultSubject => _resultSubject;

        public void PublishStartRace() => _startRaceSubject.OnNext(Unit.Default);
        public void PublishCountdownFinished() => _countdownBeforeStartFinishedSubject.OnNext(Unit.Default);

        public void PublishPlayerSpawned(ICarView carView) => _playerSpawnedSubject.OnNext(carView);

        public void PublishCarSetupChanged(CarSetupAspect carSetupAspect) => _carSetupChangedSubject.OnNext(carSetupAspect);
        public void PublishInputEnabled(bool isEnabled) => _inputEnabledSubject.OnNext(isEnabled);
        public void PublishWinResultChanged(bool isEnabled) => _resultSubject.OnNext(isEnabled);

        public void ReloadCurrentScene()
        {
            
        }

        public void Dispose()
        {
            _startRaceSubject?.OnCompleted();
            _countdownBeforeStartFinishedSubject?.OnCompleted();
            _playerSpawnedSubject?.OnCompleted();
            _carSetupChangedSubject?.OnCompleted();
            _inputEnabledSubject?.OnCompleted();

            _startRaceSubject?.Dispose();
            _countdownBeforeStartFinishedSubject?.Dispose();
            _playerSpawnedSubject?.Dispose();
            _carSetupChangedSubject?.Dispose();
            _inputEnabledSubject?.Dispose();
        }
    }
}