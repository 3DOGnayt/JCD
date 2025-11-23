using Components;
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

        private Filter _cars;
        private Stash<WheelInfoComponent> _wheelInfoStash;
        private Stash<GearboxComponent> _gearStash;
        private Stash<VerticalInputComponent> _vertStash;
        private Stash<HandbrakeInputComponent> _handbrakeStash;
        private Stash<BrakeForceComponent> _brakeForceStash;

        // Пока константы, потом можно вынести в CarMovementParameters
        private const float MaxMotorTorque      = 1500f;
        private const float HandbrakeTorqueBase = 2500f;

        public void OnAwake()
        {
            _cars = World.Filter
                .With<WheelInfoComponent>()
                .With<GearboxComponent>()
                .With<VerticalInputComponent>()
                .With<HandbrakeInputComponent>()
                .With<BrakeForceComponent>()
                .Build();

            _wheelInfoStash   = World.GetStash<WheelInfoComponent>();
            _gearStash        = World.GetStash<GearboxComponent>();
            _vertStash        = World.GetStash<VerticalInputComponent>();
            _handbrakeStash   = World.GetStash<HandbrakeInputComponent>();
            _brakeForceStash  = World.GetStash<BrakeForceComponent>();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var car in _cars)
            {
                var wheelInfo = _wheelInfoStash.Get(car);
                var gear      = _gearStash.Get(car).Value;
                var vertical  = _vertStash.Get(car).Value;
                var handbrake = _handbrakeStash.Get(car).Value;
                var brakeBase = _brakeForceStash.Get(car).Value;

                // Разделяем на газ и тормоз
                float driveInput = 0f;
                float brakeInput = 0f;

                if (gear > 0)
                {
                    if (vertical > 0f)        driveInput = Mathf.Clamp01(vertical);
                    else if (vertical < 0f)   brakeInput = -vertical;
                }
                else if (gear < 0)
                {
                    if (vertical < 0f)        driveInput = Mathf.Clamp01(-vertical); // по модулю
                    else if (vertical > 0f)   brakeInput = vertical;
                }
                else // нейтраль
                {
                    if (vertical < 0f)        brakeInput = -vertical;
                }

                // Направление двигателя: >0 вперёд, <0 назад
                float signedDriveInput = 0f;
                if (gear > 0)
                    signedDriveInput = driveInput;
                else if (gear < 0)
                    signedDriveInput = -driveInput;

                // Газ через сервис (сюда пойдут и коллайдеры, и визуалы)
                _inputService.ApplyVerticalMove(MaxMotorTorque, signedDriveInput, wheelInfo.WheelInfo);

                // Тормоза и ручник — напрямую в коллайдеры
                float brakeTorque = brakeInput * brakeBase;

                foreach (var info in wheelInfo.WheelInfo)
                {
                    if (info.LeftWheel != null)
                    {
                        info.LeftWheel.brakeTorque = brakeTorque;
                        if (handbrake && !info.Steering)
                            info.LeftWheel.brakeTorque += HandbrakeTorqueBase;
                    }

                    if (info.RightWheel != null)
                    {
                        info.RightWheel.brakeTorque = brakeTorque;
                        if (handbrake && !info.Steering)
                            info.RightWheel.brakeTorque += HandbrakeTorqueBase;
                    }
                }
            }
        }

        public void Dispose() { }
    }
}