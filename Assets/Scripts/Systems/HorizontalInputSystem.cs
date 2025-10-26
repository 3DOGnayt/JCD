using Components;
using Configs.Impl;
using Scellecs.Morpeh;
using Services;
using Signals;
using UnityEngine;
using Zenject;

namespace Systems
{
    public class HorizontalInputSystem : IFixedSystem
    {
        [Inject] public World World { get; set;}
        [Inject] private SignalBus _signalBus;
        [Inject] private CarMovementParameters _carMovementParameters;
        
        private IInputService _inputService;
        
        private Filter _cars;
        private AspectFactory<CarSetupAspect> _carSetupAspect;
        
        private Stash<WheelInfoComponent> _wheelInfoStash;
        private Stash<HorizontalInputComponent> _horizontalStash;
        private Stash<CarMassComponent> _carMassStash;

        private float _steeringSpeedMultiplierMax;
        private float _steeringSpeedMultiplierMin;
        private float _maxCarSpeed;
        private float _carMass; // rename
        private float _speedMultiplierMax;
        private float _speedMultiplierMin;

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
            _horizontalStash = World.GetStash<HorizontalInputComponent>();
            _carMassStash = World.GetStash<CarMassComponent>();

            _speedMultiplierMax = _carMovementParameters.SpeedMultiplierMax;
            _speedMultiplierMin = _carMovementParameters.SpeedMultiplierMin;
            _maxCarSpeed = _carMovementParameters.MaxCarSpeed;
            _carMass = _carMovementParameters.CarMass;
            _steeringSpeedMultiplierMax = _carMovementParameters.SteeringSpeedMultiplierMax;
            _steeringSpeedMultiplierMin = _carMovementParameters.SteeringSpeedMultiplierMin;
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var car in _cars)
            {
                var horizontal = _horizontalStash.Get(car);
                var wheelInfo = _wheelInfoStash.Get(car);
                var carMass = _carMassStash.Get(car);
                
                var carSetupAspect = _carSetupAspect.Get(car);
                ref var steeringAngle = ref carSetupAspect.SteeringAngle;
                ref var steeringSpeed = ref carSetupAspect.SteeringSpeed;
                ref var speed = ref carSetupAspect.Speed;
                
                var speedFactor = Mathf.Lerp(_speedMultiplierMax, _speedMultiplierMin, speed.Value / _maxCarSpeed);
                var massFactor = Mathf.Clamp01(_carMass / carMass.Value);

                var adjustedAngle = steeringAngle.Value * speedFactor * massFactor;
                var targetAngle = adjustedAngle * horizontal.Value;
                
                var dynamicSteeringSpeed = steeringSpeed.Value * Mathf.Lerp(_steeringSpeedMultiplierMax, _steeringSpeedMultiplierMin, speed.Value / _maxCarSpeed);

                _inputService.ApplyHorizontalMove(targetAngle, dynamicSteeringSpeed, wheelInfo.WheelInfo);
                
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