using System;
using Components;
using Signals;
using TMPro;
using UnityEngine;
using Zenject;

namespace UI
{
    public class CarParametersView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _speedText;
        [SerializeField] private TMP_Text _speedValue;
        [Space]
        [SerializeField] private TMP_Text _subParametersText;
        [SerializeField] private TMP_Text _subParametersValue;

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
            _speedText.text = "Km/h";
            _subParametersText.text = "Max Back Speed\n" + "Acceleration\n" + "Deceleration\n"
                                      + "Max Steering Angle\n" + "Steering Speed\n" + "Brake Force\n" 
                                      + "Drift Multiplier\n" + "Max Motor Torque";
        }

        private void OnPlayerSpawned(PlayerSpawnedSignal signal)
        {
            var carSetup = signal.CarView.CarPreset.CarSetup;
            var speed = carSetup.MaxSpeed;
            
            _speedValue.text = $"{speed} :";
            
            var maxBackSpeed = carSetup.MaxBackSpeed;
            var acceleration = carSetup.AccelerationMultiplier;
            var deceleration = carSetup.DecelerationMultiplier;
            var maxSteeringAngle = carSetup.MaxSteeringAngle;
            var steeringSpeed = carSetup.SteeringSpeed;
            var brakeForce = carSetup.BrakeForce;
            var driftMultiplier = carSetup.DriftMultiplier;
            var maxMotorTorque = carSetup.MaxMotorTorque;
            
            _subParametersValue.text = $"{maxBackSpeed} :\n" + $"{acceleration} :\n" + $"{deceleration} :\n"
                                       + $"{maxSteeringAngle} :\n" + $"{steeringSpeed} :\n" + $"{brakeForce} :\n" 
                                       + $"{driftMultiplier} :\n" + $"{maxMotorTorque} :";
        }

        private void OnSpeedChanged(ComponentChangeSignal<SpeedComponent> signal)
        {
            _speedValue.text = $"{signal.Component.Value:0} :";
        }
    }
}