using Data;
using Data.HelperClass;
using UnityEngine;

namespace Configs
{
    public interface ICarPreset
    {
        GameObject Car { get; }
        CarSetup CarSetup { get; }
        CarMassSetup CarMassSetup { get; }
        WheelSetup FrontWheelSetup { get; }
        WheelSetup BackWheelSetup { get; }
    }
}