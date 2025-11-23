using Components;
using Configs.Impl;
using Scellecs.Morpeh;
using UnityEngine;
using Zenject;

namespace Systems.Car.Test_Arcade
{
    public sealed class RPMSystem_A : IFixedSystem
    {
        [Inject] public World World { get; set; }
        [Inject] private CarMovementParameters _carMovementParameters;

        private Filter _cars;
        private Stash<EngineRpmComponent> _rpmStash;
        private Stash<VerticalInputComponent> _vertStash;
        private Stash<GearboxComponent> _gearStash;

        public void OnAwake()
        {
            _cars = World.Filter
                .With<EngineRpmComponent>()
                .With<VerticalInputComponent>()
                .With<GearboxComponent>()
                .Build();

            _rpmStash  = World.GetStash<EngineRpmComponent>();
            _vertStash = World.GetStash<VerticalInputComponent>();
            _gearStash = World.GetStash<GearboxComponent>();
        }

        public void OnUpdate(float deltaTime)
        {
            var idleRpm       = _carMovementParameters.IdleRpm;
            var maxRpm        = _carMovementParameters.MaxRpm;
            var neutralMaxRpm = _carMovementParameters.NeutralMaxRpm;

            foreach (var car in _cars)
            {
                ref var rpm   = ref _rpmStash.Get(car);
                var     gear  = _gearStash.Get(car).Value;
                var     input = _vertStash.Get(car).Value;

                // Стартовое значение
                if (rpm.Value <= 1f)
                    rpm.Value = idleRpm;

                var throttle = Mathf.Clamp01(Mathf.Abs(input)); // газ вперёд/назад одинаково для оборотов
                float targetRpm;

                if (gear == 0)
                {
                    // Нейтраль: от холостых до neutralMaxRpm
                    targetRpm = Mathf.Lerp(idleRpm, neutralMaxRpm, throttle);
                }
                else
                {
                    // В передаче: от холостых до красной зоны
                    targetRpm = Mathf.Lerp(idleRpm, maxRpm, throttle);
                }

                // Выбираем скорость изменения
                var isThrottle = throttle > 0.01f;
                var rate = isThrottle
                    ? _carMovementParameters.AccelerationRate
                    : _carMovementParameters.DecelerationRate;

                rpm.Value = Mathf.MoveTowards(rpm.Value, targetRpm, rate * deltaTime);
                rpm.Value = Mathf.Clamp(rpm.Value, idleRpm, maxRpm);
            }
        }

        public void Dispose() { }
    }
}