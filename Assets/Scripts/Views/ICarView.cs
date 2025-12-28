using System.Collections.Generic;
using Configs.Impl;
using Data;
using Data.Helpers;
using UnityEngine;

namespace Views
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