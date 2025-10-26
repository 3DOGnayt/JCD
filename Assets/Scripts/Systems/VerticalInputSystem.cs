using Components;
using Configs.Impl;
using Scellecs.Morpeh;
using Services;
using Signals;
using UnityEngine;
using Zenject;

namespace Systems
{
    public class VerticalInputSystem : IFixedSystem
    {
        [Inject] public World World { get; set;}
        [Inject] private SignalBus _signalBus;
        [Inject] private CarMovementParameters _carMovementParameters;
        
        private IInputService _inputService;
        
        private Filter _cars;
        private AspectFactory<CarSetupAspect> _carSetupAspect;

        private Stash<WheelInfoComponent> _wheelInfoStash;
        private Stash<VerticalInputComponent> _verticalStash;
        private Stash<CarMassComponent> _carMassStash;

        private float _accelerationRate;
        private float _decelerationRate ;
        private float _maxRpm;
        private float _idleRpm;
        private float _rpmToSpeedRatio;
        
        [Inject]
        public void Construct(IInputService inputService)
        {
            _inputService = inputService;
        }

        public void OnAwake()
        {
            _cars = World.Filter.Extend<CarSetupAspect>().Build();
            _carSetupAspect = World.GetAspectFactory<CarSetupAspect>();

            _wheelInfoStash = World.GetStash<WheelInfoComponent>();
            _verticalStash = World.GetStash<VerticalInputComponent>();
            _carMassStash = World.GetStash<CarMassComponent>();
            
            _accelerationRate = _carMovementParameters.AccelerationRate;
            _decelerationRate = _carMovementParameters.DecelerationRate;
            _maxRpm = _carMovementParameters.MaxRpm;
            _idleRpm = _carMovementParameters.IdleRpm;
            _rpmToSpeedRatio = _carMovementParameters.RpmToSpeedRatio;
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var car in _cars)
            {
                var wheelInfo = _wheelInfoStash.Get(car);
                var vertical = _verticalStash.Get(car);
                var carMass = _carMassStash.Get(car);

                var carSetupAspect = _carSetupAspect.Get(car);
                ref var motorTorque = ref carSetupAspect.MotorTorque;
                ref var speed = ref carSetupAspect.Speed;

                if (vertical.Value > 0)
                    motorTorque.Value += _accelerationRate * vertical.Value * deltaTime;
                else if (vertical.Value < 0)
                    motorTorque.Value -= _decelerationRate * Mathf.Abs(vertical.Value) * deltaTime;
                else
                    motorTorque.Value = Mathf.MoveTowards(motorTorque.Value, _idleRpm, _decelerationRate * deltaTime);

                motorTorque.Value = Mathf.Clamp(motorTorque.Value, 0f, _maxRpm);
                speed.Value = Mathf.Max(0, (motorTorque.Value - _idleRpm) * _rpmToSpeedRatio);
                
                _inputService.ApplyVerticalMove(motorTorque.Value, vertical.Value, wheelInfo.WheelInfo);
                
                _signalBus.Fire(new ComponentChangeSignal<CarSetupAspect> 
                { 
                    Entity = car, 
                    Component = carSetupAspect
                });
            }
        }

        public void Dispose() { }
    }
}