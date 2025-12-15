using Data;
using UnityEngine;

namespace Configs
{
    public interface ICarPreset
    {
        GameObject Car { get; }
        CarSetup CarSetup { get; }
        CarMassParameters CarMassParameters { get; }
        WheelParameters FrontWheelParameters { get; }
        WheelParameters BackWheelParameters { get; }
    }
}