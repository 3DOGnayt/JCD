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
        [SerializeField] private float _accelerationRate; // for rpm
        [SerializeField] private float _decelerationRate;
        [Space]
        [SerializeField] private float _maxRpm;
        [SerializeField] private float _idleRpm;
        [Space]
        [SerializeField] private SystemHelpersSetup _systemHelpersSetup;
        [Header("Arcade Assist")] 
        [Space]
        [SerializeField] private bool _useArcadeAssist = true;
        [SerializeField] private bool _useArcadeAssistInDrift = true;
        [SerializeField] private float _arcadeAssistMinSpeedKmh;
        [SerializeField] private float _arcadeAssistLerpSpeed = 1f;
        [SerializeField] private float _driftAssistForwardSpeedMultiplier = 1f;
        [Header("Air Control")]
        [Space]
        [SerializeField] private float _maxAirborneHeight = 0.5f;
        [SerializeField] private float _uprightStartAngleDeg = 10f;
        [SerializeField] private float _uprightTorque = 8f;
        [SerializeField] private float _uprightDamping = 1.5f;
        [SerializeField] private LayerMask _groundMask = ~0;
        [Header("Velocity Align")]
        [Space]
        [SerializeField] private bool _useVelocityAlign = true;
        [SerializeField] private bool _velocityAlignRequireCounterSteer = true;
        [SerializeField] private float _velocityAlignTorque = 10f;
        [SerializeField] private float _velocityAlignDamping = 2f;
        [SerializeField] private float _velocityAlignMinSpeedKmh = 10f;
        [SerializeField] private float _velocityAlignMinSlipAngleDeg = 10f;
        
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
        public bool UseArcadeAssistInDrift => _useArcadeAssistInDrift;
        public float ArcadeAssistMinSpeedKmh => _arcadeAssistMinSpeedKmh;
        public float ArcadeAssistLerpSpeed => _arcadeAssistLerpSpeed;
        public float DriftAssistForwardSpeedMultiplier => _driftAssistForwardSpeedMultiplier;
        
        public float MaxAirborneHeight => _maxAirborneHeight;
        public float UprightStartAngleDeg => _uprightStartAngleDeg;
        public float UprightTorque => _uprightTorque;
        public float UprightDamping => _uprightDamping;
        public LayerMask GroundMask => _groundMask;
        
        public bool UseVelocityAlign => _useVelocityAlign;
        public bool VelocityAlignRequireCounterSteer => _velocityAlignRequireCounterSteer;
        public float VelocityAlignTorque => _velocityAlignTorque;
        public float VelocityAlignDamping => _velocityAlignDamping;
        public float VelocityAlignMinSpeedKmh => _velocityAlignMinSpeedKmh;
        public float VelocityAlignMinSlipAngleDeg => _velocityAlignMinSlipAngleDeg;
    }
}