using System;
using Components;
using Configs.Impl;
using Scellecs.Morpeh;
using Services;
using UniRx;
using UnityEngine;
using Zenject;

namespace Systems.Car
{
    public sealed class BrakeSystem : IFixedSystem
    {
        [Inject] public World World { get; set; }

        private IEventService _eventService;
        private CarSelectionParameters _carSelectionParameters;

        private Filter _cars;
        private Stash<WheelInfoComponent> _wheelInfoStash;
        private Stash<VerticalInputComponent> _verticalInputStash;
        private Stash<SpeedComponent> _speedStash;
        private Stash<BackSpeedComponent> _backSpeedStash;
        private Stash<GearComponent> _gearStash;
        private Stash<BrakeInputComponent> _brakeInputStash;
        private Stash<HandbrakeInputComponent> _handbrakeStash;

        private bool _inputEnabled = true;
        private IDisposable _inputEnabledSubscription;

        [Inject]
        public void Construct(
            IEventService eventService,
            CarSelectionParameters carSelectionParameters)
        {
            _eventService = eventService;
            _carSelectionParameters = carSelectionParameters;
        }

        public void OnAwake()
        {
            _cars = World.Filter
                .With<WheelInfoComponent>()
                .With<VerticalInputComponent>()
                .With<SpeedComponent>()
                .With<BackSpeedComponent>()
                .With<GearComponent>()
                .With<BrakeInputComponent>()
                .With<HandbrakeInputComponent>()
                .Build();

            _wheelInfoStash = World.GetStash<WheelInfoComponent>();
            _verticalInputStash = World.GetStash<VerticalInputComponent>();
            _speedStash = World.GetStash<SpeedComponent>();
            _backSpeedStash = World.GetStash<BackSpeedComponent>();
            _gearStash = World.GetStash<GearComponent>();
            _brakeInputStash = World.GetStash<BrakeInputComponent>();
            _handbrakeStash = World.GetStash<HandbrakeInputComponent>();

            _inputEnabledSubscription = _eventService.InputEnabledStream.Subscribe(SetInputEnabled);
        }

        public void OnUpdate(float deltaTime)
        {
            var carParameters = _carSelectionParameters != null ? _carSelectionParameters.SelectedCarParameters : null;
            if (carParameters == null)
                return;

            foreach (var car in _cars)
            {
                if (!_inputEnabled)
                {
                    ref var cachedVertical = ref _verticalInputStash.Get(car).Value;
                    cachedVertical = 0f;
                }

                var verticalInput = Mathf.Clamp(_verticalInputStash.Get(car).Value, -1f, 1f);
                var forwardSpeedKmh = Mathf.Max(0f, _speedStash.Get(car).Value);
                var backwardSpeedKmh = Mathf.Max(0f, Mathf.Abs(_backSpeedStash.Get(car).Value));

                var isMovingForward = forwardSpeedKmh >= backwardSpeedKmh;
                var scalarSpeedKmh = Mathf.Max(forwardSpeedKmh, backwardSpeedKmh);
                var systemHelpers = carParameters.MovementParameters.HelpersSetup;
                var isAlmostStopped = scalarSpeedKmh < systemHelpers.StopThresholdKmh;

                var wheelInfoComponent = _wheelInfoStash.Get(car);
                var currentGear = _gearStash.Get(car).Value;
                var handbrakePressed = _handbrakeStash.Get(car).Value;

                float brakeForce;
                bool brakeInputFlag;

                ResolveBrake(
                    verticalInput,
                    currentGear,
                    isMovingForward,
                    isAlmostStopped,
                    carParameters,
                    out brakeForce,
                    out brakeInputFlag);

                _brakeInputStash.Get(car).Value = brakeInputFlag;
                ApplyBrakes(wheelInfoComponent, brakeForce, handbrakePressed, carParameters);
            }
        }

        private void ResolveBrake(
            float verticalInput,
            int currentGear,
            bool isMovingForward,
            bool isAlmostStopped,
            CarParameters carParameters,
            out float brakeForce,
            out bool brakeInputFlag)
        {
            brakeForce = 0f;
            brakeInputFlag = false;

            var systemHelpers = carParameters.MovementParameters.HelpersSetup;
            if (Mathf.Abs(verticalInput) < systemHelpers.InputDeadZone)
                return;

            var wantsForward = verticalInput > 0f;
            var wantsBackward = verticalInput < 0f;

            if (currentGear == 0)
            {
                if (!isAlmostStopped)
                    SetBrakeMode(verticalInput, out brakeForce, out brakeInputFlag);

                return;
            }

            if (wantsForward)
            {
                if (!isMovingForward && !isAlmostStopped)
                    SetBrakeMode(verticalInput, out brakeForce, out brakeInputFlag);
            }
            else if (wantsBackward)
            {
                if (isMovingForward && !isAlmostStopped)
                    SetBrakeMode(verticalInput, out brakeForce, out brakeInputFlag);
            }
        }

        private void SetBrakeMode(
            float verticalInput,
            out float brakeForce,
            out bool brakeInputFlag)
        {
            brakeForce = Mathf.Abs(verticalInput);
            brakeInputFlag = true;
        }

        private void ApplyBrakes(
            WheelInfoComponent wheelInfoComponent,
            float brakeForce,
            bool handbrakePressed,
            CarParameters carParameters)
        {
            var parameters = carParameters.MovementParameters.Vertical;
            var pedalBrakeTorque = parameters.BrakeTorque * Mathf.Max(0f, brakeForce);
            var handbrakeTorque = handbrakePressed ? parameters.HandbrakeTorque : 0f;

            foreach (var info in wheelInfoComponent.WheelInfo)
            {
                var totalBrakeTorque = pedalBrakeTorque;

                if (handbrakePressed && info.Motor)
                    totalBrakeTorque += handbrakeTorque;

                if (info.LeftWheel != null)
                    info.LeftWheel.brakeTorque = totalBrakeTorque;

                if (info.RightWheel != null)
                    info.RightWheel.brakeTorque = totalBrakeTorque;
            }
        }

        private void SetInputEnabled(bool isEnabled)
        {
            _inputEnabled = isEnabled;
        }

        public void Dispose()
        {
            _inputEnabledSubscription?.Dispose();
        }
    }
}