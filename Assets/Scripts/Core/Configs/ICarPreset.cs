using Core.Data;
using UnityEngine;

namespace Core.Configs
{
    public interface ICarPreset
    {
        GameObject Car { get; }
        CarParameters CarParameters { get; }
        WheelParameters WheelParameters { get; }
        WheelSubParameters WheelSubParameters { get; }
    }
}