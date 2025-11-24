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
        private Stash<BrakeForceComponent> _brakeForceStash;
        private Stash<HandbrakeInputComponent> _handbrakeStash;

        // Можно потом вынести в CarMovementParameters
        private const float MaxMotorTorque   = 1800f; // базовая тяга на ведущие колёса
        private const float BrakeStrength    = 500.0f;  // усиление обычного тормоза
        private const float HandbrakeTorque  = 3500f; // сила ручника на задние колёса

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
            _handbrakeStash   = World.GetStash<HandbrakeInputComponent>(); // опциональный
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

                // --- определяем, газим или тормозим ---

                if (gear > 0)
                {
                    if (vertical > 0f)
                    {
                        // едем вперёд
                        driveInput = Mathf.Clamp01(vertical);
                    }
                    else if (vertical < 0f)
                    {
                        // жмём "назад" при передней передаче -> ЭТО ТОРМОЗ
                        brakeInput = Mathf.Clamp01(-vertical);
                    }
                }
                else if (gear < 0)
                {
                    if (vertical < 0f)
                    {
                        // едем назад
                        driveInput = Mathf.Clamp01(-vertical);
                    }
                    else if (vertical > 0f)
                    {
                        // жмём "вперёд" при задней передаче -> тормоз
                        brakeInput = Mathf.Clamp01(vertical);
                    }
                }
                else // Neutral
                {
                    // на нейтрали "назад" можно использовать как тормоз
                    if (vertical < 0f)
                        brakeInput = Mathf.Clamp01(-vertical);
                }

                // --- моторная тяга только по передаче и газу ---

                float signedDriveInput = 0f;
                if (gear > 0)
                    signedDriveInput = driveInput;     // вперёд
                else if (gear < 0)
                    signedDriveInput = -driveInput;    // назад

                // если ручник зажат или мы явно тормозим -> мотор должен замолчать
                if (brakeInput > 0f || handbrake)
                    signedDriveInput = 0f;

                // тяга только на ведущие колёса — это уже заложено в InputService (info.Motor)
                _inputService.ApplyVerticalMove(MaxMotorTorque, signedDriveInput, wheelInfo.WheelInfo);

                // --- обычный тормоз: сильный brakeTorque на все колёса ---

                float brakeTorque = 0f;

                if (brakeInput > 0f)
                {
                    // усиливаем ощущение торможения
                    brakeTorque = brakeBase * brakeInput * BrakeStrength;
                }

                // --- ручник: дополнительный тормоз только на задние (не рулевые) колёса ---

                foreach (var info in wheelInfo.WheelInfo)
                {
                    // обычный тормоз на все колёса
                    if (info.LeftWheel != null)
                        info.LeftWheel.brakeTorque = brakeTorque;

                    if (info.RightWheel != null)
                        info.RightWheel.brakeTorque = brakeTorque;

                    if (!handbrake)
                        continue;

                    // ручник: добавляем сильный тормоз только на НЕ рулевые колёса (обычно задняя ось)
                    if (!info.Steering)
                    {
                        if (info.LeftWheel != null)
                            info.LeftWheel.brakeTorque += HandbrakeTorque;

                        if (info.RightWheel != null)
                            info.RightWheel.brakeTorque += HandbrakeTorque;
                    }
                }
            }
        }

        public void Dispose() { }
    }
}