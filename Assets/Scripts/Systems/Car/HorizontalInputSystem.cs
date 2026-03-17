using System;
using Components;
using Configs.Impl;
using Scellecs.Morpeh;
using Services;
using UniRx;
using UnityEngine;
using Zenject;

namespace Systems.Car
{
    public class HorizontalInputSystem : IFixedSystem
    {
        [Inject] public World World { get; set;}
        
        private IEventService _eventService;
        private IInputService _inputService;
        private CarSelectionParameters _carSelectionParameters;

        private Filter _cars;
        private AspectFactory<CarSetupAspect> _carSetupAspect;
        
        private Stash<WheelInfoComponent> _wheelInfoStash;
        private Stash<HorizontalInputComponent> _horizontalStash;
        private Stash<CarMassComponent> _carMassStash;
        private Stash<SteeringSpeedComponent> _steeringSpeedStash;
        private Stash<SteeringAngleComponent> _steeringAngleStash;
        private Stash<SpeedMaxComponent> _speedMaxStash;

        private MovementCache _movementCache;
        private bool _hasCache;
        private bool _inputEnabled = true;
        private IDisposable _inputEnabledSubscription;
        
        [Inject]
        public void Construct(
            IEventService eventService,
            IInputService inputService,
            CarSelectionParameters carSelectionParameters
        )
        {
            _eventService = eventService;
            _inputService = inputService;
            _carSelectionParameters = carSelectionParameters;
        }

        private struct MovementCache
        {
            public float SteeringSpeedMultiplierMax;
            public float SteeringSpeedMultiplierMin;
            public float CarMassStandard;
            public float SpeedMultiplierMax;
            public float SpeedMultiplierMin;

            public void ApplyFrom(CarParameters carParameters)
            {
                var movementParameters = carParameters.MovementParameters;
                var horizontal = movementParameters.Horizontal;
                SpeedMultiplierMax = horizontal.SpeedMultiplierMax;
                SpeedMultiplierMin = horizontal.SpeedMultiplierMin;
                CarMassStandard = horizontal.CarMassStandard;
                SteeringSpeedMultiplierMax = horizontal.SteeringSpeedMultiplierMax;
                SteeringSpeedMultiplierMin = horizontal.SteeringSpeedMultiplierMin;
            }
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

            TryCacheMovementParameters();
            _inputEnabledSubscription = _eventService.InputEnabledStream.Subscribe(SetInputEnabled);
        }

        public void OnUpdate(float deltaTime)
        {
            if (!_hasCache)
                TryCacheMovementParameters();

            if (!_hasCache)
                return;

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

                var speedFactor = Mathf.Lerp(_movementCache.SpeedMultiplierMax, _movementCache.SpeedMultiplierMin, speed.Value / speedMax.Value);
                var massFactor = Mathf.Clamp01(_movementCache.CarMassStandard / carMass.Value);

                var dynamicSteeringSpeed = steeringSpeed.Value * Mathf.Lerp(
                    _movementCache.SteeringSpeedMultiplierMax, _movementCache.SteeringSpeedMultiplierMin, speed.Value / speedMax.Value);

                if (!_inputEnabled)
                {
                    horizontal.Value = 0f;
                    _inputService.ApplyHorizontalMove(0f, dynamicSteeringSpeed, wheelInfo.WheelInfo);
                }
                else
                {
                    var adjustedAngle = steeringAngle.Value * speedFactor * massFactor;
                    var targetAngle = adjustedAngle * horizontal.Value;
                    _inputService.ApplyHorizontalMove(targetAngle, dynamicSteeringSpeed, wheelInfo.WheelInfo);
                }
                
                _eventService.PublishCarSetupChanged(carSetupAspect);  //TODO: replace
            }
        }

        private void TryCacheMovementParameters()
        {
            if (_carSelectionParameters == null)
                return;

            var carParameters = _carSelectionParameters.SelectedCarParameters;
            if (carParameters == null)
                return;

            _movementCache.ApplyFrom(carParameters);
            _hasCache = true;
        }

        private void SetInputEnabled(bool isEnabled)
        {
            _inputEnabled = isEnabled;
        }

        public void Dispose()
        {
            _inputEnabledSubscription?.Dispose();
        }
    }
}