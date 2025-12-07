using Components;
using Configs.Impl;
using Scellecs.Morpeh;
using Services;
using Signals;
using UnityEngine;
using Zenject;

namespace Systems.Car
{
    public class HorizontalInputSystem : IFixedSystem
    {
        [Inject] public World World { get; set;}
        [Inject] private SignalBus _signalBus;
        [Inject] private CarParameters _carParameters;
        
        private IInputService _inputService;
        
        private Filter _cars;
        private AspectFactory<CarSetupAspect> _carSetupAspect;
        
        private Stash<WheelInfoComponent> _wheelInfoStash;
        private Stash<HorizontalInputComponent> _horizontalStash;
        private Stash<CarMassComponent> _carMassStash;
        private Stash<SteeringSpeedComponent> _steeringSpeedStash;
        private Stash<SteeringAngleComponent> _steeringAngleStash;
        private Stash<SpeedMaxComponent> _speedMaxStash;

        private float _steeringSpeedMultiplierMax;
        private float _steeringSpeedMultiplierMin;
        private float _carMassStandard; 
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
            _steeringSpeedStash = World.GetStash<SteeringSpeedComponent>();
            _steeringAngleStash = World.GetStash<SteeringAngleComponent>();
            _speedMaxStash = World.GetStash<SpeedMaxComponent>();

            _speedMultiplierMax = _carParameters.MovementParameters.SpeedMultiplierMax;
            _speedMultiplierMin = _carParameters.MovementParameters.SpeedMultiplierMin;
            _carMassStandard = _carParameters.MovementParameters.CarMassStandard;
            
            _steeringSpeedMultiplierMax = _carParameters.MovementParameters.SteeringSpeedMultiplierMax;
            _steeringSpeedMultiplierMin = _carParameters.MovementParameters.SteeringSpeedMultiplierMin;
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var car in _cars)
            {
                var carSetupAspect = _carSetupAspect.Get(car);
                ref var speed = ref carSetupAspect.Speed;
                
                var horizontal = _horizontalStash.Get(car);
                var wheelInfo = _wheelInfoStash.Get(car);
                var carMass = _carMassStash.Get(car);
                var steeringSpeed = _steeringSpeedStash.Get(car);
                var steeringAngle = _steeringAngleStash.Get(car);
                var speedMax = _speedMaxStash.Get(car);

                var speedFactor = Mathf.Lerp(_speedMultiplierMax, _speedMultiplierMin, speed.Value / speedMax.Value);
                var massFactor = Mathf.Clamp01(_carMassStandard / carMass.Value);

                var adjustedAngle = steeringAngle.Value * speedFactor * massFactor;
                var targetAngle = adjustedAngle * horizontal.Value;
                
                var dynamicSteeringSpeed = steeringSpeed.Value * Mathf.Lerp(
                    _steeringSpeedMultiplierMax, _steeringSpeedMultiplierMin, speed.Value / speedMax.Value);

                _inputService.ApplyHorizontalMove(targetAngle, dynamicSteeringSpeed, wheelInfo.WheelInfo);
                
                _signalBus.Fire(new ComponentChangeSignal<CarSetupAspect> { Component = carSetupAspect });
            }
        }

        public void Dispose() { }
    }
}