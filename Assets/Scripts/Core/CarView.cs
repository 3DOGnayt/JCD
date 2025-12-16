using System.Collections.Generic;
using Configs.Impl;
using Data;
using UnityEngine;

namespace Core
{
    public class CarView : MonoBehaviour, ICarView, ITrailView
    {
        [SerializeField] private CarPreset _carPreset;
        [SerializeField] private Rigidbody _carRigidbody;
        [Space]
        [SerializeField] private CarSetup _carSetup;
        [Space]
        [SerializeField] private CarEffects _carEffects;
        [Space]
        [SerializeField] private List<WheelInfo> _wheelInfos;

        public CarPreset CarPreset => _carPreset;
        public Rigidbody CarRigidbody => _carRigidbody;
        public CarSetup CarSetup => _carSetup;
        public Transform CarTransform => transform;
        public List<WheelInfo> CarWheelInfos { get => _wheelInfos; set => _wheelInfos = value; }
        
        CarEffects ITrailView.CarEffects => _carEffects;
    }

    public interface ICarView
    {
        CarPreset CarPreset { get; }
        Rigidbody CarRigidbody { get; }
        CarSetup CarSetup { get; }
        Transform CarTransform { get; }
        List<WheelInfo> CarWheelInfos { get; }
    }

    public interface ITrailView
    {
        CarEffects CarEffects { get; }
    }
}