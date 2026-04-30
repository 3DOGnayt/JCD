using System;
using Data.Struct;
using Scellecs.Morpeh;
using UniRx;

namespace Services
{
    public interface IUnitRaceTimerService
    {
        IObservable<Entity> RaceFinishedEntityStream { get; }
        bool IsRaceActive { get; }
        bool IsFinished(Entity entity);
        void SetPlayerEntity(Entity entity);
        bool RegisterLap(Entity entity, int lapIndex, bool isFinish);
    }
}
