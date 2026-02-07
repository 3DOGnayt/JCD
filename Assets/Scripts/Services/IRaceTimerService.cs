using System;
using System.Collections.Generic;
using Data.Struct;
using UniRx;

namespace Services
{
    public interface IRaceTimerService
    {
        IObservable<RaceLapRecord> LapCompletedStream { get; }
        IObservable<Unit> RaceFinishedStream { get; }
        IReadOnlyList<RaceLapRecord> Laps { get; }
        bool IsRunning { get; }
        bool IsFinished { get; }
        float CurrentRaceTime { get; }
        float TotalRaceTime { get; }

        void StartRace();
        bool RegisterLap(int lapIndex, bool isFinish);
        void ResetRace();
    }
}
