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
        [Inject] private CarMovementParameters _params;

        private Filter _cars;
        private Stash<WheelInfoComponent> _wheelInfoStash;
        private Stash<GearboxComponent> _gearStash;
        private Stash<VerticalInputComponent> _vertStash;
        private Stash<HandbrakeInputComponent> _handbrakeStash;
        private Stash<BrakeForceComponent> _brakeForceStash;

        // Пока константы, потом можно вынести в параметры
        private const float MaxMotorTorque  = 1800f;
        private const float HandbrakeTorque = 3000f;

        public void OnAwake()
        {
            _cars = World.Filter
                .With<WheelInfoComponent>()
                .With<GearboxComponent>()
                .With<VerticalInputComponent>()
                .With<BrakeForceComponent>()
                .Build();

            _wheelInfoStash   = World.GetStash<WheelInfoComponent>();
            _gearStash        = World.GetStash<GearboxComponent>();
            _vertStash        = World.GetStash<VerticalInputComponent>();
            _brakeForceStash  = World.GetStash<BrakeForceComponent>();
            _handbrakeStash   = World.GetStash<HandbrakeInputComponent>(); // опционально
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var car in _cars)
            {
                var wheelInfo = _wheelInfoStash.Get(car);
                var gear      = _gearStash.Get(car).Value;
                var vertical  = _vertStash.Get(car).Value;
                var brakeBase = _brakeForceStash.Get(car).Value;

                // если компонента ручника нет — считаем, что он отпущен
                bool handbrake = _handbrakeStash.Has(car) && _handbrakeStash.Get(car).Value;

                float driveInput = 0f; // 0..1
                float brakeInput = 0f; // 0..1

                if (gear > 0)
                {
                    if (vertical > 0f)        driveInput = Mathf.Clamp01(vertical);
                    else if (vertical < 0f)   brakeInput = Mathf.Clamp01(-vertical);
                }
                else if (gear < 0)
                {
                    if (vertical < 0f)        driveInput = Mathf.Clamp01(-vertical);
                    else if (vertical > 0f)   brakeInput = Mathf.Clamp01(vertical);
                }
                else // Neutral
                {
                    if (vertical < 0f)        brakeInput = Mathf.Clamp01(-vertical);
                }

                // направление тяги по передаче
                float signedDriveInput = 0f;
                if (gear > 0)      signedDriveInput = driveInput;
                else if (gear < 0) signedDriveInput = -driveInput;

                // ⚡ Тяга ТОЛЬКО от инпута и передачи, без rpmFactor
                _inputService.ApplyVerticalMove(MaxMotorTorque, signedDriveInput, wheelInfo.WheelInfo);

                // Тормоза
                float brakeTorque = brakeInput * brakeBase;

                foreach (var info in wheelInfo.WheelInfo)
                {
                    if (info.LeftWheel != null)
                    {
                        info.LeftWheel.brakeTorque = brakeTorque;
                        if (handbrake && !info.Steering)
                            info.LeftWheel.brakeTorque += HandbrakeTorque;
                    }

                    if (info.RightWheel != null)
                    {
                        info.RightWheel.brakeTorque = brakeTorque;
                        if (handbrake && !info.Steering)
                            info.RightWheel.brakeTorque += HandbrakeTorque;
                    }
                }
            }
        }

        public void Dispose() { }
    }
}