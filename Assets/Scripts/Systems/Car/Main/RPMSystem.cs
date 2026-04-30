using System;
using System.Collections.Generic;
using Components;
using Configs;
using Configs.Impl;
using Data.HelperClass;
using Scellecs.Morpeh;
using Services;
using UniRx;
using UnityEngine;
using Zenject;

namespace Systems.Car.Main
{
    public sealed class RPMSystem : IFixedSystem
    {
        [Inject] public World World { get; set; }
        [Inject] private CarSelectionParameters _carSelectionParameters;
        [Inject] private IEventService _eventService;

        private Filter _cars;

        private Stash<EngineRpmComponent> _rpmStash;
        private Stash<EngineRpmMaxComponent> _rpmMaxStash;
        private Stash<SpeedComponent> _speedStash;
        private Stash<BackSpeedComponent> _backSpeedStash;
        private Stash<GearComponent> _gearStash;
        private Stash<VerticalInputComponent> _verticalInputStash;

        private struct ForwardRpmBand
        {
            public int GearValue;
            public float SpeedMinKmh;
            public float SpeedMaxKmh;
            public float RpmMin;
            public float RpmMax;
        }

        private List<ForwardRpmBand> _forwardBands;
        private Dictionary<int, int> _forwardBandIndexByGear;

        private float _reverseMaxSpeedKmh;
        private bool _hasReverseBand;
        private IDisposable _carSelectionSubscription;

        public void OnAwake()
        {
            _cars = World.Filter
                .With<EngineRpmComponent>()
                .With<EngineRpmMaxComponent>()
                .With<SpeedComponent>()
                .With<BackSpeedComponent>()
                .With<GearComponent>()
                .With<VerticalInputComponent>()
                .Build();

            _rpmStash = World.GetStash<EngineRpmComponent>();
            _rpmMaxStash = World.GetStash<EngineRpmMaxComponent>();
            _speedStash = World.GetStash<SpeedComponent>();
            _backSpeedStash = World.GetStash<BackSpeedComponent>();
            _gearStash = World.GetStash<GearComponent>();
            _verticalInputStash = World.GetStash<VerticalInputComponent>();

            var speedsPreset = _carSelectionParameters != null ? _carSelectionParameters.CarSpeedsPresetParameters : null;
            
            BuildRpmBandsFromPreset(speedsPreset);
            if (_eventService != null)
                _carSelectionSubscription = _eventService.CarSelectionChangedStream.Subscribe(_ => OnCarSelectionChanged());
        }

        private void BuildRpmBandsFromPreset(CarSpeedsPresetParameters speedsPreset)
        {
            ResetRpmBands();

            if (!TryGetSpeedSettings(speedsPreset, out var carSpeedSettings))
                return;

            var forwardSettings = new List<CarSpeedSetup>();

            foreach (var setting in carSpeedSettings)
            {
                var gearValue = (int)setting.EGear;

                if (gearValue > 0)
                    forwardSettings.Add(setting);
                else if (gearValue < 0)
                {
                    _reverseMaxSpeedKmh = Mathf.Max(_reverseMaxSpeedKmh, setting.SpeedLimit);
                    _hasReverseBand = true;
                }
            }

            if (forwardSettings.Count == 0)
            {
                Debug.LogError("RPMSystem_A: no forward gears in SpeedsPreset.");
                return;
            }

            BuildForwardBands(forwardSettings);
        }

        private void ResetRpmBands()
        {
            _forwardBands = null;
            _forwardBandIndexByGear = null;
            _reverseMaxSpeedKmh = 0f;
            _hasReverseBand = false;
        }

        private bool TryGetSpeedSettings(CarSpeedsPresetParameters speedsPreset, out List<CarSpeedSetup> carSpeedSettings)
        {
            carSpeedSettings = null;

            if (speedsPreset == null)
            {
                Debug.LogError("RPMSystem_A: SpeedsPreset is null in selection parameters.");
                return false;
            }

            carSpeedSettings = speedsPreset.CarSpeedSettings;
            if (carSpeedSettings == null || carSpeedSettings.Count == 0)
            {
                Debug.LogError("RPMSystem_A: CarSpeedSettings is null or empty in SpeedsPreset.");
                return false;
            }

            return true;
        }

        private void BuildForwardBands(List<CarSpeedSetup> forwardSettings)
        {
            forwardSettings.Sort((a, b) => a.SpeedLimit.CompareTo(b.SpeedLimit));

            _forwardBands = new List<ForwardRpmBand>(forwardSettings.Count);
            _forwardBandIndexByGear = new Dictionary<int, int>();

            for (var i = 0; i < forwardSettings.Count; i++)
            {
                var prevLimit = i == 0 ? 0f : forwardSettings[i - 1].SpeedLimit;
                var currLimit = forwardSettings[i].SpeedLimit;

                var band = new ForwardRpmBand
                {
                    GearValue = (int)forwardSettings[i].EGear,
                    SpeedMinKmh = prevLimit,
                    SpeedMaxKmh = currLimit,
                    RpmMin = forwardSettings[i].RpmMin,
                    RpmMax = forwardSettings[i].RpmMax
                };

                _forwardBands.Add(band);
                _forwardBandIndexByGear[band.GearValue] = i;
            }
        }

        public void OnUpdate(float deltaTime)
        {
            var speedsPreset = _carSelectionParameters != null
                ? _carSelectionParameters.CarSpeedsPresetParameters
                : null;
            var movementParameters = _carSelectionParameters != null
                ? _carSelectionParameters.MovementParameters
                : null;
            if (speedsPreset == null || movementParameters == null)
                return;

            if (_forwardBands == null || _forwardBands.Count == 0)
            {
                BuildRpmBandsFromPreset(speedsPreset);
                if (_forwardBands == null || _forwardBands.Count == 0)
                    return;
            }

            //TODO: Refactoring
            var vertical = movementParameters.Vertical;
            var idleRpm = vertical.IdleRpm;
            var accelRpmPerSec = vertical.AccelerationRate;
            var decelRpmPerSec = vertical.DecelerationRate;
            var neutralMaxRpm = vertical.IdleRpm;

            foreach (var car in _cars)
            {
                ref var rpmComponent = ref _rpmStash.Get(car);
                ref var rpmMaxComponent = ref _rpmMaxStash.Get(car);
                ref var speedComponent = ref _speedStash.Get(car);
                ref var backSpeedComponent = ref _backSpeedStash.Get(car);
                ref var gearComponent = ref _gearStash.Get(car);

                var verticalInput = _verticalInputStash.Get(car).Value;

                var currentRpm = rpmComponent.Value;

                var rpmMax = rpmMaxComponent.Value;
                if (rpmMax <= 0f)
                    rpmMax = vertical.MaxRpm;

                var gear = gearComponent.Value;

                float targetRpm;

                if (gear == 0)
                    targetRpm = CalculateNeutralRpm(idleRpm, neutralMaxRpm, verticalInput, movementParameters);
                else if (gear < 0)
                {
                    var reverseSpeedKmh = Mathf.Max(0f, backSpeedComponent.Value);
                    targetRpm = CalculateReverseRpm(reverseSpeedKmh, idleRpm, rpmMax);
                }
                else
                {
                    var forwardSpeedKmh = Mathf.Max(0f, speedComponent.Value);
                    targetRpm = CalculateForwardRpm(gear, forwardSpeedKmh, idleRpm, rpmMax);
                }

                var changeSpeed = targetRpm > currentRpm
                    ? accelRpmPerSec
                    : decelRpmPerSec;

                var maxStep = changeSpeed * deltaTime;

                currentRpm = Mathf.MoveTowards(currentRpm, targetRpm, maxStep);

                currentRpm = Mathf.Clamp(currentRpm, idleRpm, rpmMax);
                rpmComponent.Value = currentRpm;
            }
        }

        private float CalculateNeutralRpm(float idleRpm, float neutralMaxRpm, float verticalInput, ICarMovementParameters movementParameters)
        {
            var absInput = Mathf.Abs(verticalInput);

            var systemHelpers = movementParameters.HelpersSetup;
            if (absInput < systemHelpers.NeutralInputDeadZone)
                return idleRpm;

            var time = Mathf.Clamp01(absInput);
            return Mathf.Lerp(idleRpm, neutralMaxRpm, time);
        }

        private float CalculateReverseRpm(float reverseSpeedKmh, float idleRpm, float rpmMax)
        {
            if (!_hasReverseBand || _reverseMaxSpeedKmh <= 0f)
                return idleRpm;

            var t = Mathf.InverseLerp(0f, _reverseMaxSpeedKmh, reverseSpeedKmh);
            return Mathf.Lerp(idleRpm, rpmMax, t);
        }

        private float CalculateForwardRpm(int gear, float forwardSpeedKmh, float idleRpm, float rpmMax)
        {
            var bandIndex = GetForwardBandIndex(gear);
            if (bandIndex < 0)
                bandIndex = 0;

            var band = _forwardBands[bandIndex];

            var speedValue = Mathf.Clamp(forwardSpeedKmh, band.SpeedMinKmh, band.SpeedMaxKmh);

            var time = band.SpeedMaxKmh > band.SpeedMinKmh
                ? Mathf.InverseLerp(band.SpeedMinKmh, band.SpeedMaxKmh, speedValue)
                : 1f;

            GetForwardRpmRange(idleRpm, rpmMax, band.RpmMin, band.RpmMax, out var gearMinRpm, out var gearMaxRpm);
            return Mathf.Lerp(gearMinRpm, gearMaxRpm, time);
        }

        private static void GetForwardRpmRange(float idleRpm, float rpmMax, float presetMin, float presetMax,
            out float gearMinRpm, out float gearMaxRpm)
        {
            gearMinRpm = idleRpm;
            gearMaxRpm = rpmMax;

            if (rpmMax <= 0f)
                return;

            gearMinRpm = presetMin > 0f ? presetMin : idleRpm;
            gearMaxRpm = presetMax > 0f ? presetMax : rpmMax;
            if (gearMaxRpm < gearMinRpm)
                gearMaxRpm = gearMinRpm;
        }

        private int GetForwardBandIndex(int gearValue)
        {
            if (_forwardBandIndexByGear == null)
                return -1;

            if (_forwardBandIndexByGear.TryGetValue(gearValue, out var index))
                return index;

            return -1;
        }

        private void OnCarSelectionChanged()
        {
            var speedsPreset = _carSelectionParameters != null ? _carSelectionParameters.CarSpeedsPresetParameters : null;
            BuildRpmBandsFromPreset(speedsPreset);
        }

        public void Dispose()
        {
            _carSelectionSubscription?.Dispose();
        }
    }
}