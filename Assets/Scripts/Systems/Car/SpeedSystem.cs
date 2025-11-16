using System.Collections.Generic;
using Components;
using Configs.Impl;
using Scellecs.Morpeh;
using UnityEngine;
using Zenject;

namespace Systems.Car
{
    public class SpeedSystem : IFixedSystem
    {
        [Inject] public World World { get; set; }
        [Inject] private CarMovementParameters _carMovementParameters;

        private Filter _carFilter;
        private Stash<EngineRpmComponent> _engineRpmStash;
        private Stash<GearboxComponent> _gearStash;
        private Stash<SpeedComponent> _speedStash;

        private const float SlewKmhPerSec = 999f;
        private const float CoastDecelKmhPerSec = 8f;

        private readonly Dictionary<int, float> _kmhAtRedLine = new(8);

        public void OnAwake()
        {
            _carFilter = World.Filter
                .With<EngineRpmComponent>()
                .With<GearboxComponent>()
                .With<SpeedComponent>()
                .Build();

            _engineRpmStash = World.GetStash<EngineRpmComponent>();
            _gearStash = World.GetStash<GearboxComponent>();
            _speedStash = World.GetStash<SpeedComponent>();

            _kmhAtRedLine.Clear();
            foreach (var speedSettings in _carMovementParameters.SpeedPreset.CarSpeedSettings)
            {
                var gearIndex = (int)speedSettings.EGear;
                _kmhAtRedLine[gearIndex] = speedSettings.SpeedLimit;
            }
        }

        public void OnUpdate(float deltaTime)
        {
            var idle = Mathf.Clamp(_carMovementParameters.IdleRpm, 0f, _carMovementParameters.MaxRpm - 1f);

            foreach (var car in _carFilter)
            {
                ref var engineRpm = ref _engineRpmStash.Get(car);
                ref var gearbox = ref _gearStash.Get(car);
                ref var speed = ref _speedStash.Get(car);


                var rpm01 = Mathf.InverseLerp(idle, _carMovementParameters.MaxRpm, engineRpm.Value);

                if (gearbox.Value == 0)
                {
                    speed.Value = Mathf.MoveTowards(speed.Value, 0f, CoastDecelKmhPerSec * deltaTime);
                    continue;
                }

                if (!_kmhAtRedLine.TryGetValue(gearbox.Value, out var kmhAtRedLine))
                {
                    speed.Value = Mathf.MoveTowards(speed.Value, 0f, CoastDecelKmhPerSec * deltaTime);
                    continue;
                }

                var sign = Mathf.Sign(gearbox.Value);
                var targetKmh = kmhAtRedLine * rpm01 * sign;

                speed.Value = Mathf.MoveTowards(speed.Value, targetKmh, SlewKmhPerSec * deltaTime);
            }
        }

        public void Dispose()
        {
        }
    }
}