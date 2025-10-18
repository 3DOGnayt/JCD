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
            _signalBus.Subscribe<ComponentChangeSignal<SpeedComponent>>(OnSpeedChanged);
        }

        private void OnDisable()
        {
            _signalBus.Unsubscribe<ComponentChangeSignal<SpeedComponent>>(OnSpeedChanged);
        }

        private void Awake()
        {
            _parametersText.text = "Km/h\n" + "CurrentGear\n" + "GearCount\n" + "\n" + "Max Back Speed\n" +
                                   "Acceleration\n" + "Deceleration\n" + "Max Steering Angle\n" + "Steering Speed\n"
                                   + "Brake Force\n" + "Drift Multiplier\n" + "Max Motor Torque";
        }

        private void OnPlayerSpawned(PlayerSpawnedSignal signal)
        {
            var carSetup = signal.CarView.CarPreset.CarSetup;
            var speed = carSetup.CurrentSpeed;
            var currentGear = carSetup.CurrentGear;
            var gearCount = carSetup.GearCount;
            
            var backSpeed = carSetup.CurrentBackSpeed;
            var acceleration = carSetup.AccelerationMultiplier;
            var deceleration = carSetup.DecelerationMultiplier;
            var currentSteeringAngle = carSetup.CurrentSteeringAngle;
            var steeringSpeed = carSetup.SteeringSpeed;
            var brakeForce = carSetup.BrakeForce;
            var driftMultiplier = carSetup.DriftMultiplier;
            var currentMotorTorque = carSetup.CurrentMotorTorque;
            
            _parametersValue.text = $"{speed} :\n" + $"{currentGear} :\n" +$"{gearCount} :\n" + "\n" +
                                    $"{backSpeed} :\n" + $"{acceleration} :\n" + $"{deceleration} :\n"
                                    + $"{currentSteeringAngle} :\n" + $"{steeringSpeed} :\n" + $"{brakeForce} :\n" 
                                    + $"{driftMultiplier} :\n" + $"{currentMotorTorque} :";
        }

        //TODO: нужно придумать как обновлять данные, или сделать аспект со всеми компонентами, или разбить все данные
        private void OnSpeedChanged(ComponentChangeSignal<SpeedComponent> signal)
        {
            _parametersValue.text = $"{signal.Component.Value:0} :\n" /*+ $"{currentGear} :\n" +$"{gearCount} :\n"
                                    + "\n" + $"{backSpeed} :\n" + $"{acceleration} :\n" + $"{deceleration} :\n"
                                    + $"{currentSteeringAngle} :\n" + $"{steeringSpeed} :\n" + $"{brakeForce} :\n" 
                                    + $"{driftMultiplier} :\n" + $"{currentMotorTorque} :"*/;;
        }
    }
}