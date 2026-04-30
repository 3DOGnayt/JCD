using Data.HelperClass;
using Helpers.Car.Impl;

namespace Helpers.Car
{
    public interface IEffectsView
    {
        CarEffectsSetup CarEffectsSetup { get; }
        CarCollisionListener CollisionListener { get; }
    }
}