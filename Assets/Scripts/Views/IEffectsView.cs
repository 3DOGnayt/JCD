using Data.Helpers;
using Services.Impl;

namespace Views
{
    public interface IEffectsView
    {
        CarEffectsSetup CarEffectsSetup { get; }
        CarCollisionListener CollisionListener { get; }
    }
}