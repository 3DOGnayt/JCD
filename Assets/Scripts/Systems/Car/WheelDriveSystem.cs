using System.Collections.Generic;
using Components;
using Configs.Impl;
using Scellecs.Morpeh;
using UnityEngine;
using Zenject;

namespace Systems.Car
{
    public class WheelDriveSystem : IFixedSystem
    {
        [Inject] public World World { get; set; }
        [Inject] private CarMovementParameters _carMovementParameters;

        private Filter _filter;
        private Stash<WheelInfoComponent> _wheelStash;
        private Stash<GearboxComponent> _gearStash;
        private Stash<EngineRpmComponent> _rpmStash;
        private Stash<VerticalInputComponent> _vertStash;

        // лимиты скорости @ redline из пресета: ключ = номер передачи (-1..9), значение = км/ч
        private readonly Dictionary<int, float> _kmhAtRedline = new(12);
        private float _g1Limit = 0f; // лимит 1-й передачи для расчёта gearScale

        // Временные константы (оставляю как были)
        private const float TorqueBaseNm = 2300f;
        private const float TorqueCapNm  = 9000f;
        private const float SlipKp       = 1f;
        private const float BrakeMaxNm   = 6000f;
        private const float CoastBrakeNm = 0f;

        public void OnAwake()
        {
            _filter = World.Filter
                .With<WheelInfoComponent>()
                .With<GearboxComponent>()
                .With<EngineRpmComponent>()
                .With<VerticalInputComponent>()
                .Build();

            _wheelStash = World.GetStash<WheelInfoComponent>();
            _gearStash  = World.GetStash<GearboxComponent>();
            _rpmStash   = World.GetStash<EngineRpmComponent>();
            _vertStash  = World.GetStash<VerticalInputComponent>();

            // Собираем лимиты из SO
            _kmhAtRedline.Clear();
            _g1Limit = 0f;
            foreach (var s in _carMovementParameters.SpeedPreset.CarSpeedSettings)
            {
                int gear = (int)s.EGear;                      // -1..9
                float lim = Mathf.Max(0f, s.SpeedLimit);   // модуль
                _kmhAtRedline[gear] = lim;
                if (gear == 1) _g1Limit = lim;
            }
        }

        public void OnUpdate(float deltaTime)
        {
            float redline = _carMovementParameters.MaxRpm;

            foreach (var ent in _filter)
            {
                var wheels = _wheelStash.Get(ent);
                ref var gb  = ref _gearStash.Get(ent);
                ref var eng = ref _rpmStash.Get(ent);
                ref var vIn = ref _vertStash.Get(ent);

                // --- 1) целевая скорость от RPM/передачи (знак от передачи)
                float speedTargetKmh = 0f;
                float currentLimit = 0f;
                int absGearIndex = Mathf.Abs(gb.Value);
                if (absGearIndex >= 1 && _kmhAtRedline.TryGetValue(absGearIndex, out currentLimit) && currentLimit > 0f)
                {
                    float rpm01 = Mathf.Clamp01(eng.Value / Mathf.Max(1f, redline));
                    speedTargetKmh = currentLimit * rpm01 * Mathf.Sign(gb.Value);
                }

                // --- 2) газ/тормоз с учётом направления
                float throttle01, brake01;
                if (gb.Value > 0)
                {
                    throttle01 = Mathf.Max(0f,  vIn.Value);
                    brake01    = Mathf.Max(0f, -vIn.Value);
                }
                else if (gb.Value < 0)
                {
                    throttle01 = Mathf.Max(0f, -vIn.Value);
                    brake01    = Mathf.Max(0f,  vIn.Value);
                }
                else
                {
                    throttle01 = 0f;
                    brake01    = Mathf.Max(0f, -vIn.Value);
                }

                // --- 3) радиус и набор моторных колёс
                var motorWheels = new List<WheelCollider>();
                float radius = 0.35f;
                foreach (var info in wheels.WheelInfo)
                {
                    if (info.Motor)
                    {
                        if (info.LeftWheel  != null) motorWheels.Add(info.LeftWheel);
                        if (info.RightWheel != null) motorWheels.Add(info.RightWheel);
                    }
                }

                if (motorWheels.Count == 0)
                {
                    foreach (var info in wheels.WheelInfo)
                    {
                        SyncVisuals(info.LeftWheel,  info.LeftVisual);
                        SyncVisuals(info.RightWheel, info.RightVisual);
                    }
                    continue;
                }

                var refWheel = motorWheels[0];
                radius = Mathf.Max(radius, refWheel.radius);

                // --- 4) целевой/фактический rpm колеса
                float wheelRpmTarget = (Mathf.Abs(speedTargetKmh) / 3.6f) / radius * (60f / (2f * Mathf.PI));
                float wheelRpmActual = Mathf.Abs(refWheel.rpm);

                // --- 5) множитель передачи по пресету (от 1-й)
                float gearScale = 0f;
                if (absGearIndex >= 1 && currentLimit > 0f && _g1Limit > 0f)
                    gearScale = _g1Limit / currentLimit; // 1-я ≈1.0; дальше <1

                // --- 6) момент на колёса
                float slipErr = wheelRpmTarget - wheelRpmActual;
                float driveNmOneWheel = Mathf.Clamp(
                    (TorqueBaseNm * gearScale) * throttle01 + SlipKp * slipErr,
                    0f, TorqueCapNm);

                // знак по передаче
                float signTorque = (throttle01 > 0f)
                    ? (gb.Value >= 1 ? +1f : (gb.Value <= -1 ? -1f : 0f))
                    : 0f;

                // --- 7) равномерно раздаём момент по ведущим колёсам
                float perWheelNm = driveNmOneWheel;
                foreach (var info in wheels.WheelInfo)
                {
                    if (!info.Motor) continue;
                    if (info.LeftWheel  != null) info.LeftWheel.motorTorque  = perWheelNm * signTorque;
                    if (info.RightWheel != null) info.RightWheel.motorTorque = perWheelNm * signTorque;
                }

                // --- 8) тормоз (равномерно всем)
                float brakeNm = Mathf.Max(BrakeMaxNm * brake01, CoastBrakeNm);
                foreach (var info in wheels.WheelInfo)
                {
                    if (info.LeftWheel  != null) info.LeftWheel.brakeTorque  = brakeNm;
                    if (info.RightWheel != null) info.RightWheel.brakeTorque = brakeNm;

                    SyncVisuals(info.LeftWheel,  info.LeftVisual);
                    SyncVisuals(info.RightWheel, info.RightVisual);
                }
            }
        }

        public void Dispose() { }

        private static void SyncVisuals(WheelCollider wheel, Transform visual)
        {
            if (wheel == null || visual == null) return;
            wheel.GetWorldPose(out var pos, out var rot);
            visual.SetPositionAndRotation(pos, rot);
        }
    }
}
