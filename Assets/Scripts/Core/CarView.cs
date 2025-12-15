using System.Collections.Generic;
using Configs.Impl;
using Data;
using UnityEngine;

namespace Core
{
    public class CarView : MonoBehaviour, ICarView
    {
        [SerializeField] private CarPreset _carPreset;
        [SerializeField] private Rigidbody _carRigidbody;
        [Space]
        [SerializeField] private CarSetup _carSetup;
        [Space]
        [SerializeField] private List<WheelInfo> _wheelInfos;

        public CarPreset CarPreset => _carPreset;
        public Rigidbody CarRigidbody => _carRigidbody;
        public Transform CarTransform => transform;
        public CarSetup CarSetup => _carSetup;
        public List<WheelInfo> CarWheelInfos { get => _wheelInfos; set => _wheelInfos = value; }
    }

    public interface ICarView
    {
        CarPreset CarPreset { get; }
        Rigidbody CarRigidbody { get; }
        Transform CarTransform { get; }
        List<WheelInfo> CarWheelInfos { get; }
    }
}