using Components;
using Configs.Impl;
using Scellecs.Morpeh;
using UnityEngine;
using Zenject;

namespace Systems.Car
{
    public sealed class RPMSystem_Test : IFixedSystem
    {
        [Inject] public World World { get; set; }
        [Inject] private CarMovementParameters _carMovementParameters;

        private Filter _filter;
        private Stash<EngineRpmComponent> _rpmStash;
        private Stash<VerticalInputComponent> _vertStash;
        private Stash<GearboxComponent> _gearStash;

        // Временные константы из 1-й передачи
        private const float GrowGas = 2400f;     // об/с (газ)
        private const float DecelNoGas = 1600f;  // об/с (накат)
        private const float DecelBrake = 2200f;  // об/с (тормоз)

        public void OnAwake()
        {
            _filter = World.Filter
                .With<EngineRpmComponent>()
                .With<VerticalInputComponent>()
                .With<GearboxComponent>()
                .Build();

            _rpmStash  = World.GetStash<EngineRpmComponent>();
            _vertStash = World.GetStash<VerticalInputComponent>();
            _gearStash = World.GetStash<GearboxComponent>();
        }

        public void OnUpdate(float dt)
        {
            float idle    = _carMovementParameters.IdleRpm;
            float redline = _carMovementParameters.MaxRpm;

            foreach (var ent in _filter)
            {
                ref var rpm  = ref _rpmStash.Get(ent);
                ref var vert = ref _vertStash.Get(ent);
                ref var gb   = ref _gearStash.Get(ent);

                float v = vert.Value;

                if (gb.Value > 0) // прямые передачи
                {
                    if (v > 0f)        rpm.Value += GrowGas    * v * dt;          // газ
                    else if (v < 0f)   rpm.Value -= DecelBrake * (-v) * dt;        // тормоз
                    else               rpm.Value -= DecelNoGas * dt;               // накат
                }
                else if (gb.Value < 0) // реверс: газ — это "назад"
                {
                    if (v < 0f)        rpm.Value += GrowGas    * (-v) * dt;       // газ в R
                    else if (v > 0f)   rpm.Value -= DecelBrake * v * dt;           // тормоз в R
                    else               rpm.Value -= DecelNoGas * dt;               // накат
                }
                else // нейтраль
                {
                    rpm.Value -= DecelNoGas * dt;
                }

                rpm.Value = Mathf.Clamp(rpm.Value, idle, redline);
            }
        }

        public void Dispose() { }
    }
}
