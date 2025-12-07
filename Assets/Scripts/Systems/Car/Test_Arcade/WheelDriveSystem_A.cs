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
        [Inject] private CarParameters _carParameters;

        private Filter _cars;
        private AspectFactory<CarSetupAspect> _carAspectFactory;
        private Stash<WheelInfoComponent> _wheelInfoStash;
        private Stash<VerticalInputComponent> _vertStash;

        private const float InputDeadZone = 0.05f;
        private const float StopThresholdKmh = 0.5f;

        public void OnAwake()
        {
            _cars = World.Filter
                .Extend<CarSetupAspect>()
                .With<WheelInfoComponent>()
                .With<VerticalInputComponent>()
                .Build();

            _carAspectFactory = World.GetAspectFactory<CarSetupAspect>();
            _wheelInfoStash = World.GetStash<WheelInfoComponent>();
            _vertStash = World.GetStash<VerticalInputComponent>();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var car in _cars)
            {
                var aspect = _carAspectFactory.Get(car);
                ref var speed = ref aspect.Speed.Value;
                ref var backSpeed = ref aspect.BackSpeed.Value;
                ref var brakeInput = ref aspect.BrakeInput.Value;
                ref var handbrake = ref aspect.HandbrakeInput.Value;

                var wheelInfoComp = _wheelInfoStash.Get(car);
                var input = Mathf.Clamp(_vertStash.Get(car).Value, -1f, 1f);
                var scalarKmh = Mathf.Max(speed, Mathf.Abs(backSpeed));

                var movingForward = speed >= backSpeed;
                var almostStopped = scalarKmh < StopThresholdKmh;

                var maxTorque = 0f;
                var driveInput = 0f;
                var brakeForce = 0f;

                if (Mathf.Abs(input) < InputDeadZone)
                {
                    maxTorque = 0f;
                    driveInput = 0f;
                    brakeForce = 0f;
                    brakeInput = false;
                }
                else if (input > 0f)
                {
                    if (!movingForward && !almostStopped)
                    {
                        maxTorque = 0f;
                        driveInput = 0f;
                        brakeForce = input;
                        brakeInput = true;
                    }
                    else
                    {
                        maxTorque = _carParameters.MovementParameters.EngineForwardTorque;
                        driveInput = input;
                        brakeForce = 0f;
                        brakeInput = false;
                    }
                }
                else
                {
                    if (movingForward && !almostStopped)
                    {
                        maxTorque = 0f;
                        driveInput = 0f;
                        brakeForce = -input;
                        brakeInput = true;
                    }
                    else
                    {
                        maxTorque = _carParameters.MovementParameters.EngineBackTorque;
                        driveInput = input;
                        brakeForce = 0f;
                        brakeInput = false;
                    }
                }

                if (handbrake)
                {
                    maxTorque = 0f;
                    driveInput = 0f;
                }

                _inputService.ApplyVerticalMove(maxTorque, driveInput, wheelInfoComp.WheelInfo);

                var pedalBrakeTorque = _carParameters.MovementParameters.BrakeTorque * brakeForce;
                var hbTorque = handbrake ? _carParameters.MovementParameters.HandbrakeTorque : 0f;

                foreach (var info in wheelInfoComp.WheelInfo)
                {
                    var totalBrake = pedalBrakeTorque;

                    if (handbrake && info.Motor)
                        totalBrake += hbTorque;

                    if (info.LeftWheel != null)
                        info.LeftWheel.brakeTorque = totalBrake;

                    if (info.RightWheel != null)
                        info.RightWheel.brakeTorque = totalBrake;
                }
            }
        }
        
        public void Dispose() { }
    }
}
