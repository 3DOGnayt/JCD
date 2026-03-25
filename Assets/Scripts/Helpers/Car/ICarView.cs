using System.Collections.Generic;
using Configs.Impl;
using Data.HelperClass;
using UnityEngine;

namespace Helpers.Car
{
    public interface ICarView
    {
        CarPresetParameters CarPresetParameters { get; }
        Rigidbody CarRigidbody { get; }
        Transform CarTransform { get; }
        List<WheelInfoSetup> CarWheelInfos { get; }
    }
}