using System;
using System.Collections.Generic;
using Components;
using Configs;
using Configs.Impl;
using Scellecs.Morpeh;
using Services;
using UniRx;
using UnityEngine;
using Zenject;

namespace Systems.Car.Main
{
    public sealed class WheelDriveSystem : IFixedSystem
    {
        [Inject] public World World { get; set; }
        
        private IEventService _eventService;
        private IInputService _inputService;
        private CarSelectionParameters _carSelectionParameters;

        private Filter _cars;
        private AspectFactory<CarSetupAspect> _carAspectFactory;
        private Stash<WheelInfoComponent> _wheelInfoStash;
        private Stash<VerticalInputComponent> _verticalInputStash;

        private Dictionary<int, float> _forwardGearTorque;
        private float _reverseGearTorque;
        private bool _inputEnabled = true;

        private IDisposable _inputEnabledSubscription;
        private IDisposable _carSelectionSubscription;

        [Inject]
        public void Construct(
            IEventService eventService,
            IInputService inputService,
            CarSelectionParameters carSelectionParameters
        )
        {
            _eventService = eventService;
            _inputService = inputService;
            _carSelectionParameters = carSelectionParameters;
        }

        public void OnAwake()
        {
            _cars = World.Filter
                .Extend<CarSetupAspect>()
                .With<WheelInfoComponent>()
                .With<VerticalInputComponent>()
                .Build();

            _carAspectFactory = World.GetAspectFactory<CarSetupAspect>();
            _wheelInfoStash = World.GetStash<WheelInfoComponent>();
            _verticalInputStash = World.GetStash<VerticalInputComponent>();

            var speedsPreset = _carSelectionParameters != null ? _carSelectionParameters.CarSpeedsPresetParameters : null;
            BuildGearTorqueFromPreset(speedsPreset);

            _inputEnabledSubscription = _eventService.InputEnabledStream.Subscribe(SetInputEnabled);
            _carSelectionSubscription = _eventService.CarSelectionChangedStream.Subscribe(_ => OnCarSelectionChanged());
        }

        private void BuildGearTorqueFromPreset(CarSpeedsPresetParameters speedsPreset)
        {
            _forwardGearTorque = new Dictionary<int, float>();
            _reverseGearTorque = 0f;

            if (speedsPreset == null)
                return;

            var carSpeedSettings = speedsPreset.CarSpeedSettings;
            if (carSpeedSettings == null || carSpeedSettings.Count == 0)
            {
                Debug.LogError("WheelDriveSystem_A: CarSpeedSettings is null or empty in SpeedsPreset.");
                return;
            }

            var hasForward = false;
            var hasReverse = false;

            foreach (var speedSetting in carSpeedSettings)
            {
                var gearValue = (int)speedSetting.EGear;
                var gearMotorTorque = Mathf.Max(0f, speedSetting.SpeedAcceleration);

                if (gearValue > 0)
                {
                    _forwardGearTorque[gearValue] = gearMotorTorque;
                    hasForward = true;
                }
                else if (gearValue < 0)
                {
                    _reverseGearTorque = gearMotorTorque;
                    hasReverse = true;
                }
            }

            if (!hasForward)
            {
                Debug.LogError("WheelDriveSystem_A: no forward gear torque values defined in SpeedsPreset.");
                _forwardGearTorque.Clear();
                return;
            }

            if (!hasReverse)
            {
                Debug.LogError("WheelDriveSystem_A: no reverse gear torque value defined in SpeedsPreset.");
                _forwardGearTorque.Clear();
                return;
            }
        }

        public void OnUpdate(float deltaTime)
        {
            var speedsPreset = _carSelectionParameters != null ? _carSelectionParameters.CarSpeedsPresetParameters : null;
            var movementParameters = _carSelectionParameters != null ? _carSelectionParameters.MovementParameters : null;
            
            if (speedsPreset == null || movementParameters == null)
                return;

            if (_forwardGearTorque == null || _forwardGearTorque.Count == 0)
            {
                BuildGearTorqueFromPreset(speedsPreset);
                if (_forwardGearTorque == null || _forwardGearTorque.Count == 0)
                    return;
            }

            foreach (var car in _cars)
            {
                var aspect = _carAspectFactory.Get(car);
                var brakeInputFlag = aspect.BrakeInput.Value;
                ref var currentGear = ref aspect.Gear.Value;

                if (!_inputEnabled)
                {
                    ref var cachedVertical = ref _verticalInputStash.Get(car).Value;
                    cachedVertical = 0f;
                }

                var verticalInput = Mathf.Clamp(_verticalInputStash.Get(car).Value, -1f, 1f);
                var wheelInfoComponent = _wheelInfoStash.Get(car);

                float maxMotorTorque;
                float driveInput;

                ResolveDrive(verticalInput, currentGear, brakeInputFlag, movementParameters, out maxMotorTorque, out driveInput);
                
                _inputService.ApplyVerticalMove(maxMotorTorque, driveInput, wheelInfoComponent.WheelInfo);
            }
        }

        private void ResolveDrive(
            float verticalInput,
            int currentGear,
            bool brakeInputPressed,
            ICarMovementParameters movementParameters,
            out float maxMotorTorque,
            out float driveInput
        )
        {
            maxMotorTorque = 0f;
            driveInput = 0f;

            var systemHelpers = movementParameters.HelpersSetup;
            if (Mathf.Abs(verticalInput) < systemHelpers.InputDeadZone)
                return;

            if (brakeInputPressed)
                return;

            var wantsForward = verticalInput > 0f;
            var wantsBackward = verticalInput < 0f;

            if (currentGear == 0)
                return;

            if (wantsForward)
            {
                if (currentGear > 0)
                {
                    maxMotorTorque = GetForwardGearTorque(currentGear);
                    driveInput = verticalInput;
                }
            }
            else if (wantsBackward)
            {
                if (currentGear < 0)
                {
                    maxMotorTorque = _reverseGearTorque;
                    driveInput = verticalInput;
                }
            }
        }

        private float GetForwardGearTorque(int gearValue)
        {
            if (_forwardGearTorque != null && _forwardGearTorque.TryGetValue(gearValue, out var torque))
                return torque;

            Debug.LogError($"WheelDriveSystem_A: no torque value for forward gear {gearValue} in SpeedsPreset.");
            return 0f;
        }

        private void SetInputEnabled(bool isEnabled)
        {
            _inputEnabled = isEnabled;
        }

        private void OnCarSelectionChanged()
        {
            var speedsPreset = _carSelectionParameters != null ? _carSelectionParameters.CarSpeedsPresetParameters : null;
            BuildGearTorqueFromPreset(speedsPreset);
        }

        public void Dispose()
        {
            _inputEnabledSubscription?.Dispose();
            _carSelectionSubscription?.Dispose();
        }
    }
}