using System.Collections.Generic;
using Components;
using Configs.Helpers;
using Configs.Impl;
using Scellecs.Morpeh;
using UnityEngine;
using Zenject;

namespace Systems.Car.Test_Arcade
{
    public sealed class RPMSystem_A : IFixedSystem
    {
        [Inject] public World World { get; set; }
        [Inject] private CarParameters _params;

        private Filter _cars;
        private Stash<EngineRpmComponent> _rpmStash;
        private Stash<VerticalInputComponent> _vertStash;
        private Stash<GearComponent> _gearStash;

        private struct GearRates
        {
            public float Grow;
            public float DecNoGas;
            public float DecBrake;
        }

        private Dictionary<int, GearRates> _rates;

        public void OnAwake()
        {
            _cars = World.Filter
                .With<EngineRpmComponent>()
                .With<VerticalInputComponent>()
                .With<GearComponent>()
                .Build();

            _rpmStash  = World.GetStash<EngineRpmComponent>();
            _vertStash = World.GetStash<VerticalInputComponent>();
            _gearStash = World.GetStash<GearComponent>();

            BuildRates();
        }

        private void BuildRates()
        {
            _rates = new Dictionary<int, GearRates>();

            if (!_rates.ContainsKey((int)EGear.Neutral) && _rates.ContainsKey((int)EGear.FirstGear))
                _rates[(int)EGear.Neutral] = _rates[(int)EGear.FirstGear];

            if (!_rates.ContainsKey((int)EGear.Reverse) && _rates.ContainsKey((int)EGear.FirstGear))
                _rates[(int)EGear.Reverse] = _rates[(int)EGear.FirstGear];
        }

        private GearRates GetRates(int gear)
        {
            if (_rates != null && _rates.TryGetValue(gear, out var r))
                return r;

            // если нет данных — используем 1-ю передачу или дефолт
            if (_rates != null && _rates.TryGetValue((int)EGear.FirstGear, out var first))
                return first;

            return new GearRates
            {
                Grow = _params.MovementParameters.AccelerationRate, 
                DecNoGas = _params.MovementParameters.DecelerationRate,
                DecBrake = _params.MovementParameters.DecelerationRate * 1.2f
            };
        }

        public void OnUpdate(float deltaTime)
        {
            var idle = _params.MovementParameters.IdleRpm;
            var max  = _params.MovementParameters.MaxRpm;

            foreach (var car in _cars)
            {
                ref var rpm   = ref _rpmStash.Get(car);
                var     gear  = _gearStash.Get(car).Value;
                var     input = _vertStash.Get(car).Value;

                if (rpm.Value < idle)
                    rpm.Value = idle;

                var rates     = GetRates(gear);
                var absInput  = Mathf.Abs(input);
                var hasInput = absInput > 0.01f;

                // Тормоз (жмём назад, когда едем вперёд, или наоборот)
                var isBrakeCommand = gear != 0 && ((gear > 0 && input < -0.01f) || (gear < 0 && input > 0.01f));
                float delta;

                if (isBrakeCommand)
                    delta = -rates.DecBrake * deltaTime;
                else if (hasInput)
                    delta = rates.Grow * absInput * deltaTime;
                else
                    delta = -rates.DecNoGas * deltaTime;

                rpm.Value += delta;
                rpm.Value = Mathf.Clamp(rpm.Value, idle, max);
            }
        }

        public void Dispose() { }
    }
}