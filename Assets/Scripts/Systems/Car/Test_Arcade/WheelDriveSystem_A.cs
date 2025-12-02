using Components;
using Configs.Impl;
using Scellecs.Morpeh;
using Services;
using UnityEngine;
using Zenject;

namespace Systems.Car.Test_Arcade
{
    public sealed class WheelDriveSystem_A : IFixedSystem
    {
        [Inject] public World World { get; set; }
        [Inject] private IInputService _inputService;
        [Inject] private CarMovementParameters _carMovementParameters;

        private Filter _cars;
        private Stash<WheelInfoComponent> _wheelInfoStash;
        private Stash<VerticalInputComponent> _vertStash;
        private Stash<HandbrakeInputComponent> _handbrakeStash;

        private const float InputDeadZone = 0.05f;

        public void OnAwake()
        {
            _cars = World.Filter
                .With<WheelInfoComponent>()
                .With<VerticalInputComponent>()
                .Build();

            _wheelInfoStash = World.GetStash<WheelInfoComponent>();
            _vertStash = World.GetStash<VerticalInputComponent>();
            _handbrakeStash = World.GetStash<HandbrakeInputComponent>();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var car in _cars)
            {
                var wheelInfoComp = _wheelInfoStash.Get(car);
                var vertical = _vertStash.Get(car).Value;

                // если компоненты нет — считаем, что ручник не зажат
                var handbrake = _handbrakeStash.Has(car) && _handbrakeStash.Get(car).Value;

                var input = Mathf.Clamp(vertical, -1f, 1f);

                // ---------- 1. Моторная тяга ----------
                float maxTorque;
                float driveInput;

                if (Mathf.Abs(input) < InputDeadZone || handbrake)
                {
                    // нет газа или ручник зажат → тяги нет
                    maxTorque = 0f;
                    driveInput = 0f;
                }
                else if (input > 0f)
                {
                    // вперёд
                    maxTorque = _carMovementParameters.EngineForwardTorque;
                    driveInput = input; // [-1..1] — см. замечание к InputService ниже
                }
                else
                {
                    // назад
                    maxTorque = _carMovementParameters.EngineBackTorque;
                    driveInput = input; // отрицательный
                }

                _inputService.ApplyVerticalMove(maxTorque, driveInput, wheelInfoComp.WheelInfo);

                // ---------- 2. Ручник: тормоз только на задние (не рулевые) колёса ----------
                var hbTorque = handbrake ? _carMovementParameters.HandbrakeTorque : 0f;

                foreach (var info in wheelInfoComp.WheelInfo)
                {
                    // считаем, что задняя ось — это те, у кого Steering == false
                    if (info.LeftWheel != null)
                    {
                        if (handbrake && info.Motor)
                            info.LeftWheel.brakeTorque = hbTorque;
                        else
                            info.LeftWheel.brakeTorque = 0f;
                    }

                    if (info.RightWheel != null)
                    {
                        if (handbrake && info.Motor)
                            info.RightWheel.brakeTorque = hbTorque;
                        else
                            info.RightWheel.brakeTorque = 0f;
                    }
                }
            }
        }

        public void Dispose()
        {
        }
    }
}