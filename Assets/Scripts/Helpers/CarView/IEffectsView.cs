using Data.HelperClass;

namespace Helpers.CarView
{
    public interface IEffectsView
    {
        CarEffectsSetup CarEffectsSetup { get; }
        CarCollisionListener CollisionListener { get; }
    }
}