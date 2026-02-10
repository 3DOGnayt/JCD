using System;
using UniRx;
using Components;
using Views;

namespace Services
{
    public interface ILoadingService
    {
        IReactiveProperty<float> LoadingProgress { get; }
        IReactiveProperty<bool> IsLoadingCompleted { get; }

        IObservable<Unit> StartRaceStream { get; }
        IObservable<Unit> CountdownFinishedStream { get; }
        IObservable<ICarView> PlayerSpawnedStream { get; }
        IObservable<CarSetupAspect> CarSetupChangedStream { get; }
        IObservable<bool> InputEnabledStream { get; }

        void PublishStartRace();
        void PublishCountdownFinished();
        void PublishPlayerSpawned(ICarView carView);
        void PublishCarSetupChanged(CarSetupAspect carSetupAspect);
        void PublishInputEnabled(bool isEnabled);
        
        void ReloadCurrentScene();
    }
}