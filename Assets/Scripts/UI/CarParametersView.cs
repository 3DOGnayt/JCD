using Components;
using Signals;
using TMPro;
using UnityEngine;
using Zenject;

namespace UI
{
    public class CarParametersView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _parametersText;
        [SerializeField] private TMP_Text _parametersValue;

        private SignalBus _signalBus;

        [Inject]
        public void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
            signalBus.Subscribe<PlayerSpawnedSignal>(OnPlayerSpawned);
        }
        
        private void OnEnable()
        {
            _signalBus.Subscribe<ComponentChangeSignal<CarSetupAspect>>(OnCarSetupAspectChanged);
        }

        private void OnDisable()
        {
            _signalBus.Unsubscribe<ComponentChangeSignal<CarSetupAspect>>(OnCarSetupAspectChanged);
        }

        private void Awake()
        {
            _parametersText.text = "Km/h\n" + 
                                   "Gearbox\n" + 
                                   "\n" + 
                                   "Back Speed\n" +
                                   "Acceleration\n" + 
                                   "Deceleration\n" + 
                                   "C. Steering Angle\n" +
                                   "Steering Speed\n" + 
                                   "Brake Force\n" + 
                                   "Drift Multiplier\n" + 
                                   "C. Engine Rpm";
        }

        private void OnPlayerSpawned(PlayerSpawnedSignal signal)
        {
            var carSetup = signal.CarView.CarPreset.CarSetup;
            var speed = carSetup.CurrentSpeed;
            var gearbox = carSetup.Gearbox;
            
            var backSpeed = carSetup.CurrentBackSpeed;
            var acceleration = carSetup.AccelerationMultiplier;
            var deceleration = carSetup.DecelerationMultiplier;
            var currentSteeringAngle = carSetup.CurrentSteeringAngle;
            var steeringSpeed = carSetup.SteeringSpeed;
            var brakeForce = carSetup.BrakeForce;
            var driftMultiplier = carSetup.DriftMultiplier;
            var currentEngineRpm = carSetup.CurrentEngineRpm;
            
            _parametersValue.text = $"{speed} :\n" +
                                    $"{gearbox} :\n" + 
                                    "\n" +
                                    $"{backSpeed} :\n" + 
                                    $"{acceleration} :\n" + 
                                    $"{deceleration} :\n" +
                                    $"{currentSteeringAngle} :\n" + 
                                    $"{steeringSpeed} :\n" + 
                                    $"{brakeForce} :\n" +
                                    $"{driftMultiplier} :\n" + 
                                    $"{currentEngineRpm} :";
        }

        private void OnCarSetupAspectChanged(ComponentChangeSignal<CarSetupAspect> signal)
        {
            _parametersValue.text = $"{signal.Component.Speed.Value:0} :\n" +
                                    $"{signal.Component.Gearbox.Value:0} :\n" + 
                                    "\n" +
                                    $"{signal.Component.BackSpeed.Value:0} :\n" +
                                    $"{signal.Component.AccelerationMultiplier.Value:0} :\n" +
                                    $"{signal.Component.DecelerationMultiplier.Value:0} :\n" +
                                    $"{signal.Component.SteeringAngle.Value:0} :\n" + 
                                    $"{signal.Component.SteeringSpeed.Value:0} :\n" + 
                                    $"{signal.Component.BrakeForce.Value:0} :\n" + 
                                    $"{signal.Component.DriftMultiplier.Value:0} :\n" +
                                    $"{signal.Component.EngineRpm.Value:0} :";
        }
    }
}