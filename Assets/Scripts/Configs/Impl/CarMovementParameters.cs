using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Car/CarMovementParameters", fileName = "CarMovementParameters")]
    public class CarMovementParameters : ScriptableObject, ICarMovementParameters
    {
        [Header("HorizontalParameters")]
        [Space]
        [SerializeField] private float _speedMultiplierMax;
        [SerializeField] private float _speedMultiplierMin;
        [SerializeField] private float _carMass;
        [SerializeField] private float _steeringSpeedMultiplierMax;
        [SerializeField] private float _steeringSpeedMultiplierMin;
        [Space]
        [Header("VerticalParameters")]
        [Space]
        [SerializeField] private float _accelerationRate;
        [SerializeField] private float _decelerationRate;
        [SerializeField] private float _maxCarSpeed;
        [SerializeField] private float _maxRpm;
        [SerializeField] private float _idleRpm;
        [SerializeField] private float _rpmToSpeedRatio;
        [Space] 
        [SerializeField] private SpeedsPreset _speedPreset;

        public float SpeedMultiplierMax => _speedMultiplierMax;
        public float SpeedMultiplierMin => _speedMultiplierMin;
        public float MaxCarSpeed => _maxCarSpeed;
        public float CarMass => _carMass;
        public float SteeringSpeedMultiplierMax => _steeringSpeedMultiplierMax;
        public float SteeringSpeedMultiplierMin => _steeringSpeedMultiplierMin;

        public float AccelerationRate => _accelerationRate;
        public float DecelerationRate => _decelerationRate;
        public float MaxRpm => _maxRpm;
        public float IdleRpm => _idleRpm;
        public float RpmToSpeedRatio => _rpmToSpeedRatio;

        public SpeedsPreset SpeedPreset => _speedPreset;
    }
}