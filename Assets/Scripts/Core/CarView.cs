using System.Collections.Generic;
using Configs.Impl;
using Data;
using UnityEngine;

namespace Core
{
    public class CarView : MonoBehaviour, ICarView
    {
        [SerializeField] private CarPreset _carPreset;
        [Space]
        [SerializeField] private CarSetup _carSetup;
        [Space]
        [SerializeField] private List<WheelInfo> _wheelInfos;

        public Transform CarTransform => transform;
        public CarPreset CarPreset => _carPreset;
        public CarSetup CarSetup => _carSetup;
        public List<WheelInfo> CarWheelInfos { get => _wheelInfos; set => _wheelInfos = value; }
    }

    public interface ICarView
    {
        Transform CarTransform { get; }
        CarPreset CarPreset { get; }
        List<WheelInfo> CarWheelInfos { get; }
    }
}