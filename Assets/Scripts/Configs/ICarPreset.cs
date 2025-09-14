using Data;
using UnityEngine;

namespace Configs
{
    public interface ICarPreset
    {
        GameObject Car { get; }
        CarSetup CarSetup { get; }
        CarParameters CarParameters { get; }
        WheelParameters FrontWheelParameters { get; }
        WheelParameters BackWheelParameters { get; }
    }
}