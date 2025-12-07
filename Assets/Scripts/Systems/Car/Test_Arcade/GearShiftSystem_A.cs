using System.Collections.Generic;
using Components;
using Configs.Helpers;
using Configs.Impl;
using Data;
using Scellecs.Morpeh;
using UnityEngine;
using Zenject;

namespace Systems.Car.Test_Arcade
{
    public sealed class GearShiftSystem_A : IFixedSystem
    {
        [Inject] public World World { get; set; }
        [Inject] private CarParameters _params;

        private Filter _cars;
        private Stash<GearComponent> _gearStash;
        private Stash<SpeedComponent> _speedStash;
        private Stash<BackSpeedComponent> _backSpeedStash;
        private Stash<VerticalInputComponent> _vertStash;

        private struct GearData
        {
            public float SpeedLimitKmh;
            public GearData(float speedLimitKmh) => SpeedLimitKmh = speedLimitKmh;
        }

        private Dictionary<int, GearData> _gearData;   // int (EGear) -> данные
        private List<int> _forwardGears;              // отсортированный список передних передач

        private int _reverseGear = (int)EGear.Reverse;
        private int _neutralGear = (int)EGear.Neutral;

        private int _minForwardGear;
        private int _maxForwardGear;

        private const float StopThresholdKmh = 1.0f;   // считаем, что почти стоим
        private const float InputDeadZone    = 0.1f;

        public void OnAwake()
        {
            _cars = World.Filter
                .With<GearComponent>()
                .With<SpeedComponent>()
                .With<BackSpeedComponent>()
                .With<VerticalInputComponent>()
                .Build();

            _gearStash      = World.GetStash<GearComponent>();
            _speedStash     = World.GetStash<SpeedComponent>();
            _backSpeedStash = World.GetStash<BackSpeedComponent>();
            _vertStash      = World.GetStash<VerticalInputComponent>();

            BuildGearDataFromPreset();
        }

        private void BuildGearDataFromPreset()
        {
            _gearData     = new Dictionary<int, GearData>();
            _forwardGears = new List<int>();

            /*var preset = _params.SpeedPreset;
            if (preset == null || preset.CarSpeedSettings == null)
                return;

            foreach (var setting in preset.CarSpeedSettings)
            {
                var gearInt = (int)setting.EGear;
                _gearData[gearInt] = new GearData(setting.SpeedLimit);

                if (gearInt > 0)
                    _forwardGears.Add(gearInt);
                else if (gearInt < 0)
                    _reverseGear = gearInt;
                else
                    _neutralGear = gearInt;
            }*/

            if (_forwardGears.Count == 0)
            {
                // На всякий случай, чтобы не словить делёжку на ноль
                _forwardGears.Add(1);
            }

            _forwardGears.Sort();
            _minForwardGear = _forwardGears[0];
            _maxForwardGear = _forwardGears[_forwardGears.Count - 1];
        }

        private float GetLimitKmh(int gear)
        {
            if (_gearData != null && _gearData.TryGetValue(gear, out var data))
                return data.SpeedLimitKmh;

            return _params.MovementParameters.MaxCarSpeed; // запасной вариант
        }

        /// <summary>
        /// Выбираем переднюю передачу только по скорости:
        /// <= limit(1) -> 1-я
        /// <= limit(2) -> 2-я
        /// ...
        /// выше всех лимитов -> последняя передача
        /// </summary>
        private int GetForwardGearForSpeed(float speedKmh)
        {
            var result = _minForwardGear;

            foreach (var g in _forwardGears)
            {
                var limit = GetLimitKmh(g);
                result = g;

                if (speedKmh <= limit)
                    break;
            }

            return Mathf.Clamp(result, _minForwardGear, _maxForwardGear);
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var car in _cars)
            {
                ref var gearComp = ref _gearStash.Get(car);
                var     speedF   = _speedStash.Get(car).Value;
                var     speedB   = _backSpeedStash.Get(car).Value;
                var     input    = _vertStash.Get(car).Value;

                float speedKmh      = Mathf.Max(speedF, speedB);
                bool  movingForward = speedF >= speedB;
                float absInput      = Mathf.Abs(input);

                // -------------------------------
                // 1. Почти стоим
                // -------------------------------
                if (speedKmh < StopThresholdKmh)
                {
                    if (absInput < InputDeadZone)
                    {
                        // Стоим и газ не жмём -> нейтраль
                        gearComp.Value = _neutralGear;
                    }
                    else if (input > InputDeadZone)
                    {
                        // Старт вперёд -> первая передача
                        gearComp.Value = _minForwardGear;
                    }
                    else if (input < -InputDeadZone)
                    {
                        // Старт назад -> задняя
                        gearComp.Value = _reverseGear;
                    }

                    continue;
                }

                // -------------------------------
                // 2. Движемся вперёд
                // -------------------------------
                if (movingForward)
                {
                    // Текущую "правильную" переднюю передачу определяем по скорости
                    int targetForwardGear = GetForwardGearForSpeed(speedKmh);

                    if (input > InputDeadZone)
                    {
                        // Жмём газ вперёд -> всегда вперёд, независимо от того,
                        // что было до этого (если вдруг были в R).
                        gearComp.Value = targetForwardGear;
                    }
                    else if (input < -InputDeadZone)
                    {
                        // Жмём назад, когда едем вперёд:
                        // - ТОРМОЗИМ (WheelDriveSystem_A уже делает тормоз)
                        // - передачи "спускаются" по скорости (3 -> 2 -> 1)
                        // - НО НЕ ПЕРЕКИДЫВАЕМСЯ В R, пока не почти остановимся.
                        if (gearComp.Value > _minForwardGear)
                        {
                            gearComp.Value = targetForwardGear;
                        }
                        else
                        {
                            // Уже на первой передаче: ждём, пока почти остановимся.
                            // Переключение в R произойдёт в блоке "почти стоим" выше.
                            gearComp.Value = _minForwardGear;
                        }
                    }
                    else
                    {
                        // Газ отпущен, просто катимся вперёд:
                        // передачи всё равно следуем за скоростью.
                        gearComp.Value = targetForwardGear;
                    }

                    continue;
                }

                // -------------------------------
                // 3. Движемся назад
                // -------------------------------
                // Пока реально катимся назад, держим заднюю передачу.
                // Переключение вперёд произойдёт только,
                // когда почти остановимся (speedKmh < StopThresholdKmh)
                // и зажмём газ вперёд (см. блок 1).
                gearComp.Value = _reverseGear;
            }
        }

        public void Dispose() { }
    }
}