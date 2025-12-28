using System.Collections.Generic;
using Components;
using Configs.Impl;
using Scellecs.Morpeh;
using Services;
using UnityEngine;
using Zenject;

namespace Systems.Car
{
    public sealed class WheelDriveSystem : IFixedSystem
    {
        [Inject] public World World { get; set; }
        [Inject] private IInputService _inputService;
        [Inject] private CarParameters _carParameters;

        private Filter _cars;
        private AspectFactory<CarSetupAspect> _carAspectFactory;
        private Stash<WheelInfoComponent> _wheelInfoStash;
        private Stash<VerticalInputComponent> _verticalInputStash;

        private Dictionary<int, float> _forwardGearTorque;
        private float _reverseGearTorque;

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

            BuildGearTorqueFromPreset();
        }

        private void BuildGearTorqueFromPreset()
        {
            _forwardGearTorque = new Dictionary<int, float>();
            _reverseGearTorque = 0f;

            var speedsPreset = _carParameters.SpeedsPresetParameters;
            if (speedsPreset == null)
            {
                Debug.LogError("WheelDriveSystem_A: SpeedsPreset is null in CarParameters.");
                return;
            }

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
            if (_forwardGearTorque == null || _forwardGearTorque.Count == 0)
                return;

            foreach (var car in _cars)
            {
                var aspect = _carAspectFactory.Get(car);

                ref var speedValue = ref aspect.Speed.Value;
                ref var backSpeedValue = ref aspect.BackSpeed.Value;
                ref var brakeInputFlag = ref aspect.BrakeInput.Value;
                ref var handbrakePressed = ref aspect.HandbrakeInput.Value;
                ref var currentGear = ref aspect.Gear.Value;

                var verticalInput = Mathf.Clamp(_verticalInputStash.Get(car).Value, -1f, 1f);

                var forwardSpeedKmh = Mathf.Max(0f, speedValue);
                var backwardSpeedKmh = Mathf.Max(0f, Mathf.Abs(backSpeedValue));
                var scalarSpeedKmh = Mathf.Max(forwardSpeedKmh, backwardSpeedKmh);

                var isMovingForward = forwardSpeedKmh >= backwardSpeedKmh;
                var systemHelpers = _carParameters.MovementParameters.HelpersSetup;
                var isAlmostStopped = scalarSpeedKmh < systemHelpers.StopThresholdKmh;

                var wheelInfoComponent = _wheelInfoStash.Get(car);

                float maxMotorTorque;
                float driveInput;
                float brakeForce;

                ResolveDriveAndBrake(
                    verticalInput,
                    currentGear,
                    isMovingForward,
                    isAlmostStopped,
                    handbrakePressed,
                    out maxMotorTorque,
                    out driveInput,
                    out brakeForce,
                    out brakeInputFlag
                );

                _inputService.ApplyVerticalMove(maxMotorTorque, driveInput, wheelInfoComponent.WheelInfo);
                ApplyBrakes(wheelInfoComponent, brakeForce, handbrakePressed);
            }
        }

        private void ResolveDriveAndBrake(
            float verticalInput,
            int currentGear,
            bool isMovingForward,
            bool isAlmostStopped,
            bool handbrakePressed,
            out float maxMotorTorque,
            out float driveInput,
            out float brakeForce,
            out bool brakeInputFlag)
        {
            maxMotorTorque = 0f;
            driveInput = 0f;
            brakeForce = 0f;
            brakeInputFlag = false;

            var systemHelpers = _carParameters.MovementParameters.HelpersSetup;
            if (Mathf.Abs(verticalInput) < systemHelpers.InputDeadZone)
                return;

            var wantsForward = verticalInput > 0f;
            var wantsBackward = verticalInput < 0f;

            if (currentGear == 0)
            {
                if (!isAlmostStopped)
                    SetBrakeMode(verticalInput, out maxMotorTorque, out driveInput, out brakeForce, out brakeInputFlag);

                if (handbrakePressed)
                {
                    maxMotorTorque = 0f;
                    driveInput = 0f;
                }

                return;
            }

            if (wantsForward)
            {
                if (!isMovingForward && !isAlmostStopped)
                    SetBrakeMode(verticalInput, out maxMotorTorque, out driveInput, out brakeForce, out brakeInputFlag);
                else if (currentGear > 0)
                {
                    maxMotorTorque = GetForwardGearTorque(currentGear);
                    driveInput = verticalInput;
                }
                else
                    return;
            }
            else if (wantsBackward)
            {
                if (isMovingForward && !isAlmostStopped)
                    SetBrakeMode(verticalInput, out maxMotorTorque, out driveInput, out brakeForce, out brakeInputFlag);
                else if (currentGear < 0)
                {
                    maxMotorTorque = _reverseGearTorque;
                    driveInput = verticalInput;
                }
                else
                    return;
            }

            if (handbrakePressed)
            {
                maxMotorTorque = 0f;
                driveInput = 0f;
            }
        }

        private void SetBrakeMode(
            float verticalInput,
            out float maxMotorTorque,
            out float driveInput,
            out float brakeForce,
            out bool brakeInputFlag)
        {
            maxMotorTorque = 0f;
            driveInput = 0f;
            brakeForce = Mathf.Abs(verticalInput);
            brakeInputFlag = true;
        }

        private float GetForwardGearTorque(int gearValue)
        {
            if (_forwardGearTorque != null && _forwardGearTorque.TryGetValue(gearValue, out var torque))
                return torque;

            Debug.LogError($"WheelDriveSystem_A: no torque value for forward gear {gearValue} in SpeedsPreset.");
            return 0f;
        }

        private void ApplyBrakes(WheelInfoComponent wheelInfoComponent, float brakeForce, bool handbrakePressed)
        {
            var parameters = _carParameters.MovementParameters;
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

        public void Dispose() { }
    }
}