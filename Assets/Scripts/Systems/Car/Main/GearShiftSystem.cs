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
    public sealed class GearShiftSystem : IFixedSystem
    {
        [Inject] public World World { get; set; }
        [Inject] private CarSelectionParameters _carSelectionParameters;
        [Inject] private IEventService _eventService;

        private Filter _cars;
        private Stash<GearComponent> _gearStash;
        private Stash<SpeedComponent> _speedStash;
        private Stash<BackSpeedComponent> _backSpeedStash;
        private Stash<VerticalInputComponent> _verticalInputStash;

        private struct ForwardGearInfo
        {
            public int GearValue;
            public float SpeedLimit;
        }

        private List<ForwardGearInfo> _forwardGears;
        private Dictionary<int, int> _forwardIndexByGearValue;
        private int _reverseGearValue;
        private int _neutralGearValue;
        private IDisposable _carSelectionSubscription;

        public void OnAwake()
        {
            _cars = World.Filter
                .With<GearComponent>()
                .With<SpeedComponent>()
                .With<BackSpeedComponent>()
                .With<VerticalInputComponent>()
                .Build();

            _gearStash = World.GetStash<GearComponent>();
            _speedStash = World.GetStash<SpeedComponent>();
            _backSpeedStash = World.GetStash<BackSpeedComponent>();
            _verticalInputStash = World.GetStash<VerticalInputComponent>();

            var speedsPreset = _carSelectionParameters != null
                ? _carSelectionParameters.CarSpeedsPresetParameters
                : null;
            
            BuildGearDataFromPreset(speedsPreset);
            if (_eventService != null)
                _carSelectionSubscription = _eventService.CarSelectionChangedStream.Subscribe(_ => OnCarSelectionChanged());
        }

        private void BuildGearDataFromPreset(CarSpeedsPresetParameters speedsPreset)
        {
            _forwardGears = new List<ForwardGearInfo>();
            _forwardIndexByGearValue = new Dictionary<int, int>();
            _reverseGearValue = -1;
            _neutralGearValue = 0;

            if (speedsPreset == null)
                return;

            var carSpeedSettings = speedsPreset.CarSpeedSettings;
            if (carSpeedSettings == null || carSpeedSettings.Count == 0)
            {
                Debug.LogError("GearShiftSystem_A: CarSpeedSettings is null or empty in SpeedPreset.");
                return;
            }

            foreach (var speedSetting in carSpeedSettings)
            {
                var gearValue = (int)speedSetting.EGear;

                if (gearValue == 0)
                    _neutralGearValue = gearValue;
                else if (gearValue < 0)
                    _reverseGearValue = gearValue;
                else
                {
                    _forwardGears.Add(new ForwardGearInfo
                    {
                        GearValue = gearValue,
                        SpeedLimit = speedSetting.SpeedLimit
                    });
                }
            }

            if (_forwardGears.Count == 0)
            {
                Debug.LogError("GearShiftSystem_A: no forward gears defined in SpeedPreset.");
                return;
            }

            _forwardGears.Sort((a, b) => a.SpeedLimit.CompareTo(b.SpeedLimit));

            for (var index = 0; index < _forwardGears.Count; index++)
                _forwardIndexByGearValue[_forwardGears[index].GearValue] = index;
        }

        public void OnUpdate(float deltaTime)
        {
            var speedsPreset = _carSelectionParameters != null ? _carSelectionParameters.CarSpeedsPresetParameters : null;
            var movementParameters = _carSelectionParameters != null ? _carSelectionParameters.MovementParameters : null;
            
            if (speedsPreset == null || movementParameters == null)
                return;

            if (_forwardGears == null || _forwardGears.Count == 0)
            {
                BuildGearDataFromPreset(speedsPreset);
                if (_forwardGears == null || _forwardGears.Count == 0)
                    return;
            }

            foreach (var car in _cars)
            {
                ref var gearComponent = ref _gearStash.Get(car);
                ref var speedComponent = ref _speedStash.Get(car);
                ref var backSpeedComponent = ref _backSpeedStash.Get(car);

                var verticalInput = _verticalInputStash.Get(car).Value;
                var currentGear = gearComponent.Value;
                var forwardSpeedKmh = Mathf.Max(0.0f, speedComponent.Value);
                var backwardSpeedKmh = Mathf.Max(0.0f, backSpeedComponent.Value);

                UpdateGear(ref currentGear, forwardSpeedKmh, backwardSpeedKmh, verticalInput, movementParameters);

                gearComponent.Value = currentGear;
            }
        }

        private void UpdateGear(
            ref int currentGear,
            float forwardSpeedKmh,
            float backwardSpeedKmh,
            float verticalInput,
            ICarMovementParameters movementParameters
        )
        {
            var absoluteSpeedKmh = Mathf.Max(forwardSpeedKmh, backwardSpeedKmh);

            var helpersSetup = movementParameters.HelpersSetup;
            var wantForward = verticalInput > helpersSetup.InputDeadZone;
            var wantBackward = verticalInput < -helpersSetup.InputDeadZone;

            if (absoluteSpeedKmh < helpersSetup.StopThresholdKmh)
            {
                if (wantForward)
                    currentGear = GetFirstForwardGear();
                else if (wantBackward)
                    currentGear = _reverseGearValue;
                else
                    currentGear = _neutralGearValue;

                return;
            }

            var movingForward = forwardSpeedKmh >= backwardSpeedKmh;

            if (!movingForward)
            {
                currentGear = _reverseGearValue;
                return;
            }

            var idealIndex = SelectForwardGearIndexBySpeed(absoluteSpeedKmh);
            if (idealIndex < 0)
                return;

            var currentIndex = GetForwardGearIndex(currentGear);
            if (currentIndex < 0)
                currentIndex = idealIndex;

            if (wantForward)
                currentIndex = idealIndex;
            else
            {
                if (idealIndex < currentIndex)
                    currentIndex = idealIndex;
            }

            currentGear = _forwardGears[currentIndex].GearValue;
        }

        private int SelectForwardGearIndexBySpeed(float absoluteSpeedKmh)
        {
            if (_forwardGears == null || _forwardGears.Count == 0)
                return -1;

            var lastIndex = _forwardGears.Count - 1;

            for (var i = 0; i < _forwardGears.Count; i++)
            {
                if (absoluteSpeedKmh <= _forwardGears[i].SpeedLimit)
                    return i;
            }

            return lastIndex;
        }

        private int GetForwardGearIndex(int gearValue)
        {
            if (_forwardIndexByGearValue == null)
                return -1;

            if (_forwardIndexByGearValue.TryGetValue(gearValue, out var index))
                return index;

            return -1;
        }

        private int GetFirstForwardGear()
        {
            if (_forwardGears != null && _forwardGears.Count > 0)
                return _forwardGears[0].GearValue;

            return 1;
        }

        private void OnCarSelectionChanged()
        {
            var speedsPreset = _carSelectionParameters != null ? _carSelectionParameters.CarSpeedsPresetParameters : null;
            BuildGearDataFromPreset(speedsPreset);
        }

        public void Dispose()
        {
            _carSelectionSubscription?.Dispose();
        }
    }
}