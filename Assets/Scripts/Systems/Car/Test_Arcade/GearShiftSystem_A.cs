using System.Collections.Generic;
using Components;
using Configs.Helpers;
using Configs.Impl;
using Data;
using Scellecs.Morpeh;
using UnityEngine;
using Zenject;

namespace Systems.Car.Test_Arcade
{
    public sealed class GearShiftSystem_A : IFixedSystem
    {
        [Inject] public World World { get; set; }
        [Inject] private CarMovementParameters _params;

        private Filter _cars;
        private Stash<GearboxComponent> _gearStash;
        private Stash<SpeedComponent> _speedStash;
        private Stash<BackSpeedComponent> _backSpeedStash;
        private Stash<VerticalInputComponent> _vertStash;
        private Stash<EngineRpmComponent> _rpmStash;

        private struct GearData
        {
            public float SpeedLimit;

            public GearData(float limit)
            {
                SpeedLimit = limit;
            }
        }

        private Dictionary<int, GearData> _gearData;
        private int _reverseGear = (int)EGear.Reverse;
        private int _minForwardGear = (int)EGear.FirstGear;
        private int _maxForwardGear = (int)EGear.FirstGear;

        private const float StopThresholdKmh   = 0.5f;
        private const float UpshiftFactor      = 0.95f; // апшифт ближе к лимиту
        private const float DownshiftFactor    = 0.7f;  // дауншифт ниже лимита предыдущей передачи

        public void OnAwake()
        {
            _cars = World.Filter
                .With<GearboxComponent>()
                .With<SpeedComponent>()
                .With<BackSpeedComponent>()
                .With<VerticalInputComponent>()
                .With<EngineRpmComponent>()
                .Build();

            _gearStash      = World.GetStash<GearboxComponent>();
            _speedStash     = World.GetStash<SpeedComponent>();
            _backSpeedStash = World.GetStash<BackSpeedComponent>();
            _vertStash      = World.GetStash<VerticalInputComponent>();
            _rpmStash       = World.GetStash<EngineRpmComponent>();

            BuildGearData();
        }

        private void BuildGearData()
        {
            _gearData = new Dictionary<int, GearData>();

            var preset = _params.SpeedPreset;
            if (preset == null || preset.CarSpeedSettings == null)
                return;

            foreach (var s in preset.CarSpeedSettings)
            {
                var gearInt = (int)s.EGear;
                _gearData[gearInt] = new GearData(s.SpeedLimit);

                if (gearInt > 0)
                {
                    if (gearInt < _minForwardGear) _minForwardGear = gearInt;
                    if (gearInt > _maxForwardGear) _maxForwardGear = gearInt;
                }
                else if (gearInt < 0)
                {
                    _reverseGear = gearInt;
                }
            }
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var car in _cars)
            {
                ref var gear = ref _gearStash.Get(car);
                ref var rpm  = ref _rpmStash.Get(car);

                var speed = _speedStash.Get(car).Value;
                var back  = _backSpeedStash.Get(car).Value;
                var input = _vertStash.Get(car).Value;

                var absInput = Mathf.Abs(input);
                var kmh      = Mathf.Max(speed, back);
                var stopped  = kmh < StopThresholdKmh;
                var wantFwd  = input > 0.1f;
                var wantBack = input < -0.1f;

                // 1. Стоим почти на месте — выбор передачи по направлению
                if (stopped)
                {
                    if (absInput < 0.1f)
                    {
                        gear.Value = (int)EGear.Neutral;
                    }
                    else if (wantFwd)
                    {
                        gear.Value = _minForwardGear;
                    }
                    else if (wantBack)
                    {
                        gear.Value = _reverseGear;
                    }

                    rpm.Value = _params.IdleRpm;
                    continue;
                }

                // 2. Едем вперёд (по модулю скорость вперёд больше)
                if (speed >= back)
                {
                    if (gear.Value <= 0)
                        gear.Value = _minForwardGear;

                    // Апшифт вперёд
                    if (gear.Value >= _minForwardGear && gear.Value < _maxForwardGear)
                    {
                        float limitThis = GetLimitKmh(gear.Value);

                        if (limitThis > 1f && kmh > limitThis * UpshiftFactor && wantFwd)
                        {
                            int oldGear = gear.Value;
                            gear.Value = Mathf.Min(gear.Value + 1, _maxForwardGear);
                            RemapRpmOnShift(ref rpm, kmh, oldGear, gear.Value);
                            continue;
                        }
                    }

                    // Дауншифт при замедлении без газа
                    if (gear.Value > _minForwardGear)
                    {
                        float limitPrev = GetLimitKmh(gear.Value - 1);
                        if (limitPrev > 1f && kmh < limitPrev * DownshiftFactor && !wantFwd)
                        {
                            int oldGear = gear.Value;
                            gear.Value = Mathf.Max(gear.Value - 1, _minForwardGear);
                            RemapRpmOnShift(ref rpm, kmh, oldGear, gear.Value);
                        }
                    }
                }
                // 3. Едем назад
                else
                {
                    if (gear.Value >= 0)
                        gear.Value = _reverseGear;
                    // одной задней передачи достаточно
                }
            }
        }

        private float GetLimitKmh(int gear)
        {
            if (_gearData != null && _gearData.TryGetValue(gear, out var data))
                return data.SpeedLimit;

            return _params.MaxCarSpeed; // запасной вариант
        }

        private void RemapRpmOnShift(ref EngineRpmComponent rpm, float kmh, int oldGear, int newGear)
        {
            var idle = _params.IdleRpm;
            var max  = _params.MaxRpm;

            float oldLimit = Mathf.Max(1f, GetLimitKmh(oldGear));
            float t        = Mathf.Clamp01(kmh / oldLimit);
            float fromSpeed = Mathf.Lerp(idle, max, t);

            // При апшифте делаем небольшой провал
            if (newGear > oldGear)
                fromSpeed *= 0.85f;

            rpm.Value = Mathf.Clamp(fromSpeed, idle, max);
        }

        public void Dispose() { }
    }
}