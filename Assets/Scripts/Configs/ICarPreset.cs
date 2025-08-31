using Data;
using UnityEngine;

namespace Configs
{
    public interface ICarPreset
    {
        GameObject Car { get; }
        CarParameters CarParameters { get; }
        WheelParameters WheelParameters { get; }
    }
}