using System.Collections.Generic;
using Components;
using Configs.Impl;
using Scellecs.Morpeh;
using Services;
using UnityEngine;
using Zenject;

namespace Systems.Car
{
    public class WheelDriveSystem : IFixedSystem
    {
        [Inject] public World World { get; set; }
        [Inject] private CarMovementParameters _carMovementParameters;
        [Inject] private IInputService _inputService;

        private Filter _filter;
        private Stash<WheelInfoComponent> _wheelStash;
        private Stash<GearboxComponent> _gearStash;
        private Stash<EngineRpmComponent> _rpmStash;
        private Stash<VerticalInputComponent> _vertStash;

        // Таблица скоростей @8000 rpm для 1..6 (N34)
        private static readonly float[] KmhAtRedline = { 73f, 118f, 165f, 212f, 278f, 350f };

        // Временные константы (вынесем в SO позже)
        private const float TorqueBaseNm = 1500f;   // было 450
        private const float TorqueCapNm  = 6000f;   // было 1200
        private const float SlipKp       = 0.8f;    // было 0.25
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
        }

        public void OnUpdate(float deltaTime)
        {
            float redline = _carMovementParameters.MaxRpm; // напр. 8000

            foreach (var ent in _filter)
            {
                var wheels = _wheelStash.Get(ent);
                ref var gb = ref _gearStash.Get(ent);
                ref var eng = ref _rpmStash.Get(ent);
                ref var vIn = ref _vertStash.Get(ent);

                // --- 1) целевая скорость от RPM/передачи (знак от передачи)
                float speedTargetKmh = 0f;
                int absGearIndex = Mathf.Abs(gb.Value);
                if (absGearIndex >= 1)
                {
                    int gi = Mathf.Clamp(absGearIndex - 1, 0, KmhAtRedline.Length - 1);
                    float rpm01 = Mathf.Clamp01(eng.Value / Mathf.Max(1f, redline));
                    speedTargetKmh = KmhAtRedline[gi] * rpm01 * Mathf.Sign(gb.Value);
                }

                // --- 2) газ/тормоз с учётом направления
                float throttle01, brake01;
                if (gb.Value > 0)
                {
                    // 1..N
                    throttle01 = Mathf.Max(0f, vIn.Value);
                    brake01 = Mathf.Max(0f, -vIn.Value);
                }
                else if (gb.Value < 0)
                {
                    // -1
                    throttle01 = Mathf.Max(0f, -vIn.Value);
                    brake01 = Mathf.Max(0f, vIn.Value);
                }
                else
                {
                    // 0
                    throttle01 = 0f;
                    brake01 = Mathf.Max(0f, -vIn.Value);
                }

                // --- 3) радиус и rpm эталон
                var motorWheels = new List<WheelCollider>();
                float radius = 0.35f;
                foreach (var info in wheels.WheelInfo)
                {
                    if (info.Motor)
                    {
                        if (info.LeftWheel != null) motorWheels.Add(info.LeftWheel);
                        if (info.RightWheel != null) motorWheels.Add(info.RightWheel);
                    }
                }

                if (motorWheels.Count == 0)
                {
                    foreach (var info in wheels.WheelInfo)
                    {
                        SyncVisuals(info.LeftWheel, info.LeftVisual);
                        SyncVisuals(info.RightWheel, info.RightVisual);
                    }

                    continue;
                }

                var refWheel = motorWheels[0];
                radius = Mathf.Max(radius, refWheel.radius);

                // --- 4) целевой/фактический rpm колеса
                float wheelRpmTarget = (Mathf.Abs(speedTargetKmh) / 3.6f) / radius * (60f / (2f * Mathf.PI));
                float wheelRpmActual = Mathf.Abs(refWheel.rpm);

                // --- 5) множитель передачи: больше Nm на коротких передачах
                int giForScale = Mathf.Clamp(absGearIndex - 1, 0, KmhAtRedline.Length - 1);
                float gearScale =
                    (absGearIndex >= 1) ? (KmhAtRedline[0] / KmhAtRedline[giForScale]) : 0f; // 1-я ≈1.0; 6-я ~0.21

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
                float perWheelNm = driveNmOneWheel; // можно делить/умножать — зависит от твоей трактовки
                foreach (var info in wheels.WheelInfo)
                {
                    if (info.Motor)
                    {
                        if (info.LeftWheel != null) info.LeftWheel.motorTorque = perWheelNm * signTorque;
                        if (info.RightWheel != null) info.RightWheel.motorTorque = perWheelNm * signTorque;
                    }
                }

                // --- 8) тормоз (равномерно всем)
                float brakeNm = Mathf.Max(BrakeMaxNm * brake01, CoastBrakeNm);
                foreach (var info in wheels.WheelInfo)
                {
                    if (info.LeftWheel != null) info.LeftWheel.brakeTorque = brakeNm;
                    if (info.RightWheel != null) info.RightWheel.brakeTorque = brakeNm;

                    SyncVisuals(info.LeftWheel, info.LeftVisual);
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