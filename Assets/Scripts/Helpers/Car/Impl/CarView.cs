using System.Collections.Generic;
using Configs.Impl;
using Data.HelperClass;
using UnityEngine;

namespace Helpers.Car.Impl
{
    public class CarView : MonoBehaviour, ICarView, IEffectsView
    {
        [SerializeField] private CarPresetParameters _carPresetParameters;
        [SerializeField] private Rigidbody _carRigidbody;
        [Space]
        [SerializeField] private CarEffectsSetup _carEffectsSetup;
        [Space]
        [SerializeField] private CarCollisionListener _carCollisionListener;
        [Space]
        [SerializeField] private List<WheelInfoSetup> _wheelInfos;

        public CarPresetParameters CarPresetParameters => _carPresetParameters;
        public Rigidbody CarRigidbody => _carRigidbody;
        public Transform CarTransform => transform;
        public List<WheelInfoSetup> CarWheelInfos { get => _wheelInfos; set => _wheelInfos = value; }
        
        CarEffectsSetup IEffectsView.CarEffectsSetup => _carEffectsSetup;
        CarCollisionListener IEffectsView.CollisionListener => _carCollisionListener;
    }
}