using Data.Helpers;
using Services.Impl;
using Systems;

namespace Views
{
    public interface IEffectsView
    {
        CarEffectsSetup CarEffectsSetup { get; }
        CarCollisionListener CollisionListener { get; }
    }
}