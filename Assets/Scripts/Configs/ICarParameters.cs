using Configs.Impl;

namespace Configs
{
    public interface ICarParameters
    {
        public CarMovementParameters MovementParameters { get; }
        public SpeedsPresetParameters SpeedsPresetParameters { get; }
    }
}