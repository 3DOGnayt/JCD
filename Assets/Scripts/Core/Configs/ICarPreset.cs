using Core.Data;

namespace Core.Configs
{
    public interface ICarPreset
    {
        CarParameters CarParameters { get; }
        WheelParameters WheelParameters { get; }
        WheelSubParameters WheelSubParameters { get; }
    }
}