using Data;
using UnityEngine;

namespace Configs
{
    public interface ICarPreset
    {
        GameObject Car { get; }
        CarParameters CarParameters { get; }
        WheelParameters FrontWheelParameters { get; }
        WheelParameters BackWheelParameters { get; }
    }
}