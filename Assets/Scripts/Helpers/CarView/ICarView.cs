using System.Collections.Generic;
using Configs.Impl;
using Data.HelperClass;
using UnityEngine;

namespace Helpers.CarView
{
    public interface ICarView
    {
        CarPresetParameters CarPresetParameters { get; }
        Rigidbody CarRigidbody { get; }
        CarSetup CarSetup { get; }
        Transform CarTransform { get; }
        List<WheelInfoSetup> CarWheelInfos { get; }
    }
}