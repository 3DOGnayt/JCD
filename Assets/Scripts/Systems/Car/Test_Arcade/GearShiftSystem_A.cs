using Components;
using Scellecs.Morpeh;
using UnityEngine;
using Zenject;

namespace Systems.Car.Test_Arcade
{
    public sealed class GearShiftSystem_A : IFixedSystem
    {
        [Inject] public World World { get; set; }

        private Filter _cars;
        private Stash<GearboxComponent> _gearStash;
        private Stash<SpeedComponent> _speedStash;
        private Stash<BackSpeedComponent> _backSpeedStash;
        private Stash<VerticalInputComponent> _vertStash;

        private const float InputDeadZone           = 0.1f;
        private const float DirectionChangeMaxKmh   = 5f;   // при большей скорости не даём резко сменить направление

        public void OnAwake()
        {
            _cars = World.Filter
                .With<GearboxComponent>()
                .With<SpeedComponent>()
                .With<BackSpeedComponent>()
                .With<VerticalInputComponent>()
                .Build();

            _gearStash      = World.GetStash<GearboxComponent>();
            _speedStash     = World.GetStash<SpeedComponent>();
            _backSpeedStash = World.GetStash<BackSpeedComponent>();
            _vertStash      = World.GetStash<VerticalInputComponent>();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var car in _cars)
            {
                ref var gear   = ref _gearStash.Get(car);
                var     speed  = _speedStash.Get(car).Value;
                var     back   = _backSpeedStash.Get(car).Value;
                var     input  = _vertStash.Get(car).Value;

                var absInput = Mathf.Abs(input);
                var stopped  = speed < 0.1f && back < 0.1f;

                // Нейтраль, если отпустили газ и почти стоим
                if (absInput < InputDeadZone)
                {
                    if (stopped)
                        gear.Value = 0;
                    continue;
                }

                // Вперёд
                if (input > 0f)
                {
                    // Не даём включить DRIVE, если ещё сильно катимся назад
                    if (back < DirectionChangeMaxKmh)
                        gear.Value = 1;
                }
                // Назад
                else if (input < 0f)
                {
                    if (speed < DirectionChangeMaxKmh)
                        gear.Value = -1;
                }
            }
        }

        public void Dispose() { }
    }
}