using System;
using UniRx;
using Components;
using Cameras;
using Helpers.Car;

namespace Services
{
    public interface IEventService
    {
        IReactiveProperty<float> LoadingProgress { get; }
        IReactiveProperty<bool> IsLoadingCompleted { get; }
        IReactiveProperty<bool> IsGameStarted { get; }
        IReactiveProperty<bool> IsTimersRefreshed { get; }


        IObservable<Unit> StartRaceStream { get; }
        IObservable<Unit> CountdownFinishedStream { get; }
        IObservable<ICarView> PlayerSpawnedStream { get; }
        IObservable<MinimapCameraHolder> MinimapSpawnedStream { get; }
        IObservable<CarSetupAspect> CarSetupChangedStream { get; }
        IObservable<Unit> CarSelectionChangedStream { get; }
        IObservable<bool> InputEnabledStream { get; }
        IObservable<bool> ResultSubject { get; }

        void PublishStartRace();
        void PublishCountdownFinished();
        void PublishPlayerSpawned(ICarView carView);
        void PublishMinimapSpawned(MinimapCameraHolder minimapCamera);
        void PublishCarSetupChanged(CarSetupAspect carSetupAspect);
        void PublishCarSelectionChanged();
        void PublishInputEnabled(bool isEnabled);
        void PublishWinResultChanged(bool isEnabled);
        void PublishGameStarted(bool isEnabled);
        void PublishTimersRefreshed(bool isEnabled);
        
        void ResetEvents();
    }
}