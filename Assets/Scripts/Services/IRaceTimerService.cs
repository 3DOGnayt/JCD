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
        bool RaceIsFinished { get; }
        float CurrentRaceTime { get; }
        float CurrentLapTime { get; }
        float TotalRaceTime { get; }

       void ResetRace();
    }
}