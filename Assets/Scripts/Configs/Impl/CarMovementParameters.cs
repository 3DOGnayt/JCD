using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Car/CarMovementParameters", fileName = "CarMovementParameters")]
    public class CarMovementParameters : ScriptableObject, ICarMovementParameters
    {
        [Header("HorizontalParameters")]
        [Space]
        [SerializeField] private float _speedMultiplierMax = 1;
        [SerializeField] private float _speedMultiplierMin = 0.3f;
        [SerializeField] private float _carMassStandart = 1500;
        [SerializeField] private float _steeringSpeedMultiplierMax = 1;
        [SerializeField] private float _steeringSpeedMultiplierMin = 0.1f;
        [Space]
        [Header("VerticalParameters")]
        [Space]
        [SerializeField] private float _maxMotorTorque;
        [SerializeField] private float _engineForwardTorque;
        [SerializeField] private float _engineBackTorque;
        [SerializeField] private float _handbrakeTorque;
        [Space]
        [SerializeField] private float _neutralMaxRpm;
        [SerializeField] private float _accelerationRate;
        [SerializeField] private float _decelerationRate;
        [Space]
        [SerializeField] private float _maxCarSpeed;
        [SerializeField] private float _maxRpm;
        [SerializeField] private float _idleRpm;
        [SerializeField] private float _rpmToSpeedRatio;
        [Space]
        [SerializeField] private SpeedsPreset _speedPreset;
        [Space] 
        [Header("Arcade Assist")] 
        [Space]
        [SerializeField] private bool _useArcadeAssist = true;
        [SerializeField] private float _arcadeAssistMinSpeedKmh = 40f;
        [SerializeField] private float _arcadeAssistLerpSpeed = 4f;
        public bool UseArcadeAssist => _useArcadeAssist;
        public float ArcadeAssistMinSpeedKmh => _arcadeAssistMinSpeedKmh;
        public float ArcadeAssistLerpSpeed => _arcadeAssistLerpSpeed;

        public float SpeedMultiplierMax => _speedMultiplierMax;
        public float SpeedMultiplierMin => _speedMultiplierMin;
        public float MaxCarSpeed => _maxCarSpeed;
        public float CarMass => _carMassStandart;
        public float SteeringSpeedMultiplierMax => _steeringSpeedMultiplierMax;
        public float SteeringSpeedMultiplierMin => _steeringSpeedMultiplierMin;

        public float MaxMotorTorque => _maxMotorTorque;
        public float EngineForwardTorque => _engineForwardTorque;
        public float EngineBackTorque => _engineBackTorque;
        public float HandbrakeTorque => _handbrakeTorque;

        public float AccelerationRate => _accelerationRate;
        public float DecelerationRate => _decelerationRate;
        public float MaxRpm => _maxRpm;
        public float IdleRpm => _idleRpm;
        public float RpmToSpeedRatio => _rpmToSpeedRatio;

        public SpeedsPreset SpeedPreset => _speedPreset;

        public float NeutralMaxRpm => _neutralMaxRpm;
    }
}