using Data.HelperClass;
using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Car/" + nameof(CarMovementParameters), fileName = nameof(CarMovementParameters))]
    public class CarMovementParameters : ScriptableObject, ICarMovementParameters
    {
        [Header("HorizontalParameters")]
        [Space]
        [SerializeField] private float _speedMultiplierMax = 1;
        [SerializeField] private float _speedMultiplierMin = 0.3f;
        [SerializeField] private float _carMassStandard = 1500;
        [SerializeField] private float _steeringSpeedMultiplierMax = 1;
        [SerializeField] private float _steeringSpeedMultiplierMin = 0.1f;
        [Space]
        [Header("VerticalParameters")]
        [Space]
        [SerializeField] private float _handbrakeTorque;
        [SerializeField] private float _brakeTorque;
        [Space]
        [SerializeField] private float _accelerationRate;
        [SerializeField] private float _decelerationRate;
        [Space]
        [SerializeField] private float _maxRpm;
        [SerializeField] private float _idleRpm;
        [Space]
        [SerializeField] private SystemHelpersSetup _systemHelpersSetup;
        [Header("Arcade Assist")] 
        [Space]
        [SerializeField] private bool _useArcadeAssist = true;
        [SerializeField] private float _arcadeAssistMinSpeedKmh;
        [SerializeField] private float _arcadeAssistLerpSpeed = 1f;
        
        public float SpeedMultiplierMax => _speedMultiplierMax;
        public float SpeedMultiplierMin => _speedMultiplierMin;
        public float CarMassStandard => _carMassStandard;
        public float SteeringSpeedMultiplierMax => _steeringSpeedMultiplierMax;
        public float SteeringSpeedMultiplierMin => _steeringSpeedMultiplierMin;

        public float HandbrakeTorque => _handbrakeTorque;
        public float BrakeTorque => _brakeTorque;

        public float AccelerationRate => _accelerationRate;
        public float DecelerationRate => _decelerationRate;
        
        public float MaxRpm => _maxRpm;
        public float IdleRpm => _idleRpm;
        
        public SystemHelpersSetup HelpersSetup => _systemHelpersSetup;
        
        public bool UseArcadeAssist => _useArcadeAssist;
        public float ArcadeAssistMinSpeedKmh => _arcadeAssistMinSpeedKmh;
        public float ArcadeAssistLerpSpeed => _arcadeAssistLerpSpeed;
    }
}