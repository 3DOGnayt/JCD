// using Components;
// using Configs.Impl;
// using Scellecs.Morpeh;
// using Services;
// using Signals;
// using UnityEngine;
// using Zenject;
//
// namespace Systems.Car
// {
//     public class VerticalInputSystem : IFixedSystem
//     {
//         [Inject] public World World { get; set;}
//         [Inject] private SignalBus _signalBus;
//         [Inject] private CarMovementParameters _carMovementParameters;
//         
//         private IInputService _inputService;
//         
//         private Filter _cars;
//         private AspectFactory<CarSetupAspect> _carSetupAspect;
//
//         private Stash<WheelInfoComponent> _wheelInfoStash;
//         private Stash<VerticalInputComponent> _verticalStash;
//         private Stash<CarMassComponent> _carMassStash;
//
//         private float _accelerationRate;
//         private float _decelerationRate ;
//         private float _maxRpm;
//         private float _idleRpm;
//         private float _rpmToSpeedRatio;
//         private float _targetRpm;
//         
//         [Inject]
//         public void Construct(IInputService inputService)
//         {
//             _inputService = inputService;
//         }
//
//         public void OnAwake()
//         {
//             _cars = World.Filter.Extend<CarSetupAspect>().Build();
//             _carSetupAspect = World.GetAspectFactory<CarSetupAspect>();
//
//             _wheelInfoStash = World.GetStash<WheelInfoComponent>();
//             _verticalStash = World.GetStash<VerticalInputComponent>();
//             _carMassStash = World.GetStash<CarMassComponent>();
//             
//             _accelerationRate = _carMovementParameters.AccelerationRate;
//             _decelerationRate = _carMovementParameters.DecelerationRate;
//             _maxRpm = _carMovementParameters.MaxRpm;
//             _idleRpm = _carMovementParameters.IdleRpm;
//             _rpmToSpeedRatio = _carMovementParameters.RpmToSpeedRatio;
//         }
//
//         public void OnUpdate(float deltaTime)
//         {
//             foreach (var car in _cars)
//             {
//                 var wheelInfo = _wheelInfoStash.Get(car);
//                 var vertical = _verticalStash.Get(car);
//                 var carMass = _carMassStash.Get(car);
//
//                 var carSetupAspect = _carSetupAspect.Get(car);
//                 ref var motorTorque = ref carSetupAspect.EngineRpm;
//                 ref var speed = ref carSetupAspect.Speed;
//
//                 if (vertical.Value > 0)
//                 {
//                     _targetRpm += _accelerationRate * vertical.Value * deltaTime * (1500f / carMass.Value);
//                 }
//                 else if (vertical.Value < 0)
//                 {
//                     if (speed.Value > 1f)
//                         _targetRpm -= 800 * Mathf.Abs(vertical.Value) * deltaTime;
//                     else 
//                         _targetRpm -= _accelerationRate * Mathf.Abs(vertical.Value) * deltaTime;
//                 }
//                 else
//                 {
//                     _targetRpm = Mathf.MoveTowards(motorTorque.Value, _idleRpm, _decelerationRate * deltaTime);
//                 }
//
//                 motorTorque.Value = Mathf.Clamp(_targetRpm, -_maxRpm * 0.5f, _maxRpm);
//
//                 speed.Value = (motorTorque.Value - _idleRpm) * _rpmToSpeedRatio;
//
//                 _inputService.ApplyVerticalMove(motorTorque.Value, vertical.Value, wheelInfo.WheelInfo);
//
//                 _signalBus.Fire(new ComponentChangeSignal<CarSetupAspect>
//                 {
//                     Entity = car,
//                     Component = carSetupAspect
//                 });
//             }
//         }
//
//         public void Dispose() { }
//     }
// }