using Data.HelperClass;

namespace Helpers.Car
{
    public interface IEffectsView
    {
        CarEffectsSetup CarEffectsSetup { get; }
        CarCollisionListener CollisionListener { get; }
    }
}