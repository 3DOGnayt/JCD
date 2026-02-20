using System.Collections.Generic;
using Components;
using Configs.Impl;
using Scellecs.Morpeh;
using UnityEngine;
using Zenject;

namespace Systems.Car
{
    public sealed class RPMSystem : IFixedSystem
    {
        [Inject] public World World { get; set; }
        [Inject] private CarSelectionParameters _carSelectionParameters;

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
        }

        private List<ForwardRpmBand> _forwardBands;
        private Dictionary<int, int> _forwardBandIndexByGear;

        private float _reverseMaxSpeedKmh;
        private bool _hasReverseBand;

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

            var carParameters = _carSelectionParameters != null ? _carSelectionParameters.SelectedCarParameters : null;
            BuildRpmBandsFromPreset(carParameters);
        }

        private void BuildRpmBandsFromPreset(CarParameters carParameters)
        {
            _forwardBands = null;
            _forwardBandIndexByGear = null;
            _reverseMaxSpeedKmh = 0f;
            _hasReverseBand = false;

            if (carParameters == null)
                return;

            var speedsPreset = carParameters.CarSpeedsPresetParameters;
            if (speedsPreset == null)
            {
                Debug.LogError("RPMSystem_A: SpeedsPreset is null in CarParameters.");
                return;
            }

            var carSpeedSettings = speedsPreset.CarSpeedSettings;
            if (carSpeedSettings == null || carSpeedSettings.Count == 0)
            {
                Debug.LogError("RPMSystem_A: CarSpeedSettings is null or empty in SpeedsPreset.");
                return;
            }

            var forwardTemp = new List<(int gear, float limit)>();

            foreach (var setting in carSpeedSettings)
            {
                var gearValue = (int)setting.EGear;

                if (gearValue > 0)
                {
                    forwardTemp.Add((gearValue, setting.SpeedLimit));
                }
                else if (gearValue < 0)
                {
                    _reverseMaxSpeedKmh = Mathf.Max(_reverseMaxSpeedKmh, setting.SpeedLimit);
                    _hasReverseBand = true;
                }
            }

            if (forwardTemp.Count == 0)
            {
                Debug.LogError("RPMSystem_A: no forward gears in SpeedsPreset.");
                return;
            }

            forwardTemp.Sort((a, b) => a.limit.CompareTo(b.limit));

            _forwardBands = new List<ForwardRpmBand>(forwardTemp.Count);
            _forwardBandIndexByGear = new Dictionary<int, int>();

            for (var i = 0; i < forwardTemp.Count; i++)
            {
                var prevLimit = i == 0 ? 0f : forwardTemp[i - 1].limit;
                var currLimit = forwardTemp[i].limit;

                var band = new ForwardRpmBand
                {
                    GearValue = forwardTemp[i].gear,
                    SpeedMinKmh = prevLimit,
                    SpeedMaxKmh = currLimit
                };

                _forwardBands.Add(band);
                _forwardBandIndexByGear[band.GearValue] = i;
            }
        }

        public void OnUpdate(float deltaTime)
        {
            var carParameters = _carSelectionParameters != null ? _carSelectionParameters.SelectedCarParameters : null;
            if (carParameters == null)
                return;

            if (_forwardBands == null || _forwardBands.Count == 0)
            {
                BuildRpmBandsFromPreset(carParameters);
                if (_forwardBands == null || _forwardBands.Count == 0)
                    return;
            }

            //TODO: Refactoring
            var movement = carParameters.MovementParameters;
            var idleRpm = movement.IdleRpm;
            var accelRpmPerSec = movement.AccelerationRate;
            var decelRpmPerSec = movement.DecelerationRate;
            var neutralMaxRpm = movement.IdleRpm;

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
                    rpmMax = movement.MaxRpm;

                var gear = gearComponent.Value;

                float targetRpm;

                if (gear == 0)
                {
                    targetRpm = CalculateNeutralRpm(idleRpm, neutralMaxRpm, verticalInput, carParameters);
                }
                else if (gear < 0)
                {
                    var reverseSpeedKmh = Mathf.Max(0f, backSpeedComponent.Value);
                    targetRpm = CalculateReverseRpm(reverseSpeedKmh, idleRpm, rpmMax);
                }
                else
                {
                    var forwardSpeedKmh = Mathf.Max(0f, speedComponent.Value);
                    targetRpm = CalculateForwardRpm(gear, forwardSpeedKmh, idleRpm, rpmMax, carParameters);
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

        private float CalculateNeutralRpm(
            float idleRpm,
            float neutralMaxRpm,
            float verticalInput,
            CarParameters carParameters)
        {
            var absInput = Mathf.Abs(verticalInput);

            var systemHelpers = carParameters.MovementParameters.HelpersSetup;
            if (absInput < systemHelpers.NeutralInputDeadZone)
                return idleRpm;

            var t = Mathf.Clamp01(absInput);
            return Mathf.Lerp(idleRpm, neutralMaxRpm, t);
        }

        private float CalculateReverseRpm(
            float reverseSpeedKmh,
            float idleRpm,
            float rpmMax)
        {
            if (!_hasReverseBand || _reverseMaxSpeedKmh <= 0f)
                return idleRpm;

            var t = Mathf.InverseLerp(0f, _reverseMaxSpeedKmh, reverseSpeedKmh);
            return Mathf.Lerp(idleRpm, rpmMax, t);
        }

        private float CalculateForwardRpm(
            int gear,
            float forwardSpeedKmh,
            float idleRpm,
            float rpmMax,
            CarParameters carParameters)
        {
            var bandIndex = GetForwardBandIndex(gear);
            if (bandIndex < 0)
                bandIndex = 0;

            var band = _forwardBands[bandIndex];

            var v = Mathf.Clamp(forwardSpeedKmh, band.SpeedMinKmh, band.SpeedMaxKmh);

            var t = band.SpeedMaxKmh > band.SpeedMinKmh
                ? Mathf.InverseLerp(band.SpeedMinKmh, band.SpeedMaxKmh, v)
                : 1f;

            var systemHelpers = carParameters.MovementParameters.HelpersSetup;
            var gearMinRpm = bandIndex == 0
                ? idleRpm
                : rpmMax * systemHelpers.UpshiftRpmDropFactor;

            return Mathf.Lerp(gearMinRpm, rpmMax, t);
        }

        private int GetForwardBandIndex(int gearValue)
        {
            if (_forwardBandIndexByGear == null)
                return -1;

            if (_forwardBandIndexByGear.TryGetValue(gearValue, out var index))
                return index;

            return -1;
        }

        public void Dispose() { }
    }
}