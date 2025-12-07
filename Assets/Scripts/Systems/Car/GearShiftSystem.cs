using System.Collections.Generic;
using Components;
using Configs.Impl;
using Scellecs.Morpeh;
using UnityEngine;
using Zenject;

namespace Systems.Car
{
    public class GearShiftSystem : IFixedSystem
    {
        [Inject] public World World { get; set; }
        [Inject] private CarParameters _carParameters;

        private Filter _filter;
        private Stash<EngineRpmComponent> _engineRpmStash;
        private Stash<GearComponent> _gearStash;
        private Stash<SpeedComponent> _speedStash;
        private Stash<VerticalInputComponent> _verticalStash;

        private readonly Dictionary<int, float> _kmhAtRedline = new(10);
        private int _maxForwardGear = 1;

        private const float ThrottleThresh = 0.10f;
        private const float StopKmh = 0.5f;
        private const float DownshiftMarginKmh = 2f;

        // ★ новые пороги для апшифта по скорости
        private const float UpshiftSpeedRatio = 0.95f;
        private const float UpshiftThrottleMin = 0.30f;

        public void OnAwake()
        {
            _filter = World.Filter
                .With<EngineRpmComponent>()
                .With<GearComponent>()
                .With<SpeedComponent>()
                .With<VerticalInputComponent>()
                .Build();

            _engineRpmStash = World.GetStash<EngineRpmComponent>();
            _gearStash = World.GetStash<GearComponent>();
            _speedStash = World.GetStash<SpeedComponent>();
            _verticalStash = World.GetStash<VerticalInputComponent>();

            _kmhAtRedline.Clear();
            _maxForwardGear = 1;

            /*foreach (var s in _carMovementParameters.SpeedPreset.CarSpeedSettings)
            {
                var gear = (int)s.EGear;
                var limit = Mathf.Max(0f, s.SpeedLimit);
                _kmhAtRedline[gear] = limit;

                if (gear >= 1 && gear > _maxForwardGear)
                    _maxForwardGear = gear;
            }*/
        }

        public void OnUpdate(float dt)
        {
            var redline = _carParameters.MovementParameters.MaxRpm;
            var idle = _carParameters.MovementParameters.IdleRpm;

            foreach (var ent in _filter)
            {
                ref var eng = ref _engineRpmStash.Get(ent);
                ref var gb = ref _gearStash.Get(ent);
                ref var sp = ref _speedStash.Get(ent);
                ref var vert = ref _verticalStash.Get(ent);

                // === Neutral (0)
                if (gb.Value == 0)
                {
                    if (vert.Value > ThrottleThresh)
                    {
                        gb.Value = 1;
                        eng.Value = RpmFromSpeed(sp.Value, 1, redline, idle);
                        continue;
                    }

                    if (vert.Value < -ThrottleThresh)
                    {
                        gb.Value = -1;
                        eng.Value = RpmFromSpeed(sp.Value, 1, redline, idle);
                        continue;
                    }

                    eng.Value = Mathf.Max(eng.Value, idle);
                    continue;
                }

                // === Reverse (-1)
                if (gb.Value < 0)
                {
                    if (sp.Value <= StopKmh && Mathf.Abs(vert.Value) <= ThrottleThresh)
                    {
                        gb.Value = 0;
                        eng.Value = Mathf.Max(idle, eng.Value);
                        continue;
                    }

                    if (sp.Value <= StopKmh && vert.Value > ThrottleThresh)
                    {
                        gb.Value = 1;
                        eng.Value = RpmFromSpeed(sp.Value, 1, redline, idle);
                    }

                    continue;
                }

                // === Forward gears (1..N)
                var current = gb.Value;

                if (sp.Value <= StopKmh && Mathf.Abs(vert.Value) <= ThrottleThresh)
                {
                    gb.Value = 0;
                    eng.Value = Mathf.Max(idle, eng.Value);
                    continue;
                }

                if (sp.Value <= StopKmh && vert.Value < -ThrottleThresh)
                {
                    gb.Value = 0;
                    eng.Value = Mathf.Max(idle, eng.Value);
                    continue;
                }

                // ★ апшифт: по redline ИЛИ по скорости при нажатом газе
                if (current < _maxForwardGear)
                {
                    var limCurr = GetLimit(current);
                    bool gas = vert.Value >= UpshiftThrottleMin;

                    if (eng.Value >= redline - 1f
                        || (gas && limCurr > 0f && sp.Value >= limCurr * UpshiftSpeedRatio))
                    {
                        gb.Value = current + 1;
                        eng.Value = RpmFromSpeed(sp.Value, gb.Value, redline, idle);
                        continue;
                    }
                }

                // дауншифт по скорости с запасом вниз
                if (current > 1)
                {
                    var lowerLim = GetLimit(current - 1);
                    if (lowerLim > 0f && sp.Value <= lowerLim - DownshiftMarginKmh)
                    {
                        gb.Value = current - 1;
                        eng.Value = RpmFromSpeed(sp.Value, gb.Value, redline, idle);
                    }
                }
            }
        }

        private float RpmFromSpeed(float kmh, int gear, float redline, float idle)
        {
            var g = Mathf.Clamp(gear, -1, _maxForwardGear);
            var lim = GetLimit(g >= 1 ? g : 1);
            var rpm = (lim <= 0f) ? idle : (kmh / lim) * redline;
            return Mathf.Clamp(rpm, idle, redline);
        }

        private float GetLimit(int gear)
        {
            return _kmhAtRedline.TryGetValue(gear, out var lim) ? lim : 0f;
        }

        public void Dispose()
        {
        }
    }
}