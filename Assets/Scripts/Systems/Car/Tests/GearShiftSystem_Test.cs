using Components;
using Configs.Impl;
using Scellecs.Morpeh;
using UnityEngine;
using Zenject;

namespace Systems.Car
{
    public class GearShiftSystem_Test : IFixedSystem
    {
        [Inject] public World World { get; set; }
        [Inject] private CarMovementParameters _carMovementParameters;

        private Filter _filter;
        private Stash<EngineRpmComponent> _engineRpmStash;
        private Stash<GearboxComponent> _gearStash;
        private Stash<SpeedComponent> _speedStash;
        private Stash<VerticalInputComponent> _verticalStash;

        // @8000 rpm для 1..6
        private static readonly float[] KmhAtRedline = { 73f, 118f, 165f, 212f, 278f, 350f };

        private const float ThrottleThresh = 0.10f;
        private const float StopKmh = 0.5f;
        private const float DownshiftHysteresisKmh = 0.5f;

        public void OnAwake()
        {
            _filter = World.Filter
                .With<EngineRpmComponent>()
                .With<GearboxComponent>()
                .With<SpeedComponent>()
                .With<VerticalInputComponent>()
                .Build();

            _engineRpmStash = World.GetStash<EngineRpmComponent>();
            _gearStash      = World.GetStash<GearboxComponent>();
            _speedStash     = World.GetStash<SpeedComponent>();
            _verticalStash  = World.GetStash<VerticalInputComponent>();
        }

        public void OnUpdate(float dt)
        {
            float redline = _carMovementParameters.MaxRpm;
            float idle    = _carMovementParameters.IdleRpm;

            foreach (var ent in _filter)
            {
                ref var eng  = ref _engineRpmStash.Get(ent);
                ref var gb   = ref _gearStash.Get(ent);
                ref var sp   = ref _speedStash.Get(ent);
                ref var vert = ref _verticalStash.Get(ent);

                // === Нейтраль (0): выбор направления
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

                // === Реверс (-1)
                if (gb.Value < 0)
                {
                    // отпущено/нажато вперёд и почти стоим -> в нейтраль
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

                // === Прямые передачи (1..N)
                int gi = Mathf.Clamp(gb.Value - 1, 0, KmhAtRedline.Length - 1);

                // отпустили / жмём назад и почти стоим -> в нейтраль
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

                // Апшифт по редлайну
                if (eng.Value >= redline - 1f && gb.Value < KmhAtRedline.Length)
                {
                    gb.Value++;
                    int newGi = Mathf.Clamp(gb.Value - 1, 0, KmhAtRedline.Length - 1);
                    eng.Value = RemapRpmForNewGear(sp.Value, newGi, redline, idle);
                    continue;
                }

                // === Дауншифт по скорости (БЕЗ требования «газ вперёд»)
                // Если скорость опустилась ниже безопасной для нижней передачи — понижаем.
                if (gb.Value > 1)
                {
                    float maxSafeDownKmh = KmhAtRedline[gi - 1]; // напр. 2→1: 73
                    if (sp.Value <= maxSafeDownKmh + DownshiftHysteresisKmh)
                    {
                        gb.Value--;
                        int newGi = Mathf.Clamp(gb.Value - 1, 0, KmhAtRedline.Length - 1);
                        eng.Value = RemapRpmForNewGear(sp.Value, newGi, redline, idle);
                        // не делаем continue здесь умышленно — пусть на следующем кадре проверит ещё раз и продолжит 4→3→2→1
                    }
                }
            }
        }

        public void Dispose() { }

        private static float RpmFromSpeed(float kmh, int gearIndex1Based, float redline, float idle)
        {
            int gi = Mathf.Clamp(gearIndex1Based - 1, 0, KmhAtRedline.Length - 1);
            float rpm = (KmhAtRedline[gi] <= 0f) ? idle : (kmh / KmhAtRedline[gi]) * redline;
            return Mathf.Clamp(rpm, idle, redline);
        }

        private static float RemapRpmForNewGear(float kmh, int newGi, float redline, float idle)
        {
            float rpm = (KmhAtRedline[newGi] <= 0f) ? idle : (kmh / KmhAtRedline[newGi]) * redline;
            return Mathf.Clamp(rpm, idle, redline);
        }
    }
}
