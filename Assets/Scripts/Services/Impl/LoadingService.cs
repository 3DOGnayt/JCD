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
        private readonly Subject<ICarView> _playerSpawnedSubject = new();
        private readonly Subject<CarSetupAspect> _carSetupChangedSubject = new();
        
        public IReactiveProperty<float> LoadingProgress => _loadingProgress;
        public IReactiveProperty<bool> IsLoadingCompleted => _isLoadingCompleted;
        public IObservable<Unit> StartRaceStream => _startRaceSubject;
        public IObservable<ICarView> PlayerSpawnedStream => _playerSpawnedSubject;
        public IObservable<CarSetupAspect> CarSetupChangedStream => _carSetupChangedSubject;

        public void PublishStartRace() => _startRaceSubject.OnNext(Unit.Default);

        public void PublishPlayerSpawned(ICarView carView) => _playerSpawnedSubject.OnNext(carView);

        public void PublishCarSetupChanged(CarSetupAspect carSetupAspect) => _carSetupChangedSubject.OnNext(carSetupAspect);

        public void ReloadCurrentScene()
        {
            
        }

        public void Dispose()
        {
            _startRaceSubject?.OnCompleted();
            _playerSpawnedSubject?.OnCompleted();
            _carSetupChangedSubject?.OnCompleted();

            _startRaceSubject?.Dispose();
            _playerSpawnedSubject?.Dispose();
            _carSetupChangedSubject?.Dispose();
        }
    }
}
