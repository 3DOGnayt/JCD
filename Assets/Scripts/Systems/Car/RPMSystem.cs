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
        [Inject] private CarMovementParameters _carMovementParameters;

        private Filter _filter;
        private Stash<EngineRpmComponent> _rpmStash;
        private Stash<VerticalInputComponent> _vertStash;
        private Stash<GearComponent> _gearStash;

        struct Rates
        {
            public float Grow;
            public float DecNoGas;
            public float DecBrake;
        }

        private readonly Dictionary<int, Rates> _rates = new(12);

        public void OnAwake()
        {
            _filter = World.Filter
                .With<EngineRpmComponent>()
                .With<VerticalInputComponent>()
                .With<GearComponent>()
                .Build();

            _rpmStash = World.GetStash<EngineRpmComponent>();
            _vertStash = World.GetStash<VerticalInputComponent>();
            _gearStash = World.GetStash<GearComponent>();

            BuildRatesFromPreset();
        }

        private void BuildRatesFromPreset()
        {
            _rates.Clear();

            foreach (var s in _carMovementParameters.SpeedPreset.CarSpeedSettings)
            {
                var gear = (int)s.EGear;
                _rates[gear] = new Rates
                {
                    Grow = s.SpeedAcceleration,
                    DecNoGas = s.SpeedDecelerationNoGas,
                    DecBrake = s.SpeedDecelerationBrake
                };
            }

            var g1 = _rates.TryGetValue(1, out var tmp1)
                ? tmp1
                : new Rates { Grow = 2400f, DecNoGas = 1600f, DecBrake = 2200f };

            if (_rates.TryGetValue(0, out var n))
            {
                n.Grow = 0f;
                if (n.DecNoGas <= 0f) n.DecNoGas = g1.DecNoGas;
                if (n.DecBrake <= 0f) n.DecBrake = g1.DecBrake;
                _rates[0] = n;
            }
            else
            {
                _rates[0] = new Rates { Grow = 0f, DecNoGas = g1.DecNoGas, DecBrake = g1.DecBrake };
            }

            if (_rates.TryGetValue(-1, out var r))
            {
                if (r.Grow <= 0f) r.Grow = g1.Grow;
                if (r.DecNoGas <= 0f) r.DecNoGas = g1.DecNoGas;
                if (r.DecBrake <= 0f) r.DecBrake = g1.DecBrake;
                _rates[-1] = r;
            }
            else
            {
                _rates[-1] = g1;
            }
        }

        public void OnUpdate(float dt)
        {
            var idle = _carMovementParameters.IdleRpm;
            var redline = _carMovementParameters.MaxRpm;

            foreach (var ent in _filter)
            {
                ref var rpm = ref _rpmStash.Get(ent);
                ref var vert = ref _vertStash.Get(ent);
                ref var gb = ref _gearStash.Get(ent);

                var gear = gb.Value;
                var v = vert.Value;

                var tryGetValue = _rates.TryGetValue(1, out var r1);
                var getRates = tryGetValue ? r1 : new Rates { Grow = 2400f, DecNoGas = 1600f, DecBrake = 2200f };
                var rates = _rates.TryGetValue(gear, out var rr) ? rr : getRates;

                if (gear > 0)
                {
                    if (v > 0f)
                        rpm.Value += rates.Grow * v * dt;
                    else if (v < 0f)
                        rpm.Value -= rates.DecBrake * (-v) * dt;
                    else
                        rpm.Value -= rates.DecNoGas * dt;
                }
                else if (gear < 0)
                {
                    if (v < 0f)
                        rpm.Value += rates.Grow * (-v) * dt;
                    else if (v > 0f)
                        rpm.Value -= rates.DecBrake * v * dt;
                    else
                        rpm.Value -= rates.DecNoGas * dt;
                }
                else
                {
                    rpm.Value -= rates.DecNoGas * dt;
                }

                rpm.Value = Mathf.Clamp(rpm.Value, idle, redline);
            }
        }

        public void Dispose()
        {
        }
    }
}