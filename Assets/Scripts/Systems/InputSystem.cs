using Components;
using Scellecs.Morpeh;
using Services;
using UnityEngine;
using Zenject;

namespace Systems
{
    public sealed class InputSystem : ISystem 
    {
        [Inject] public World World { get; set;}
        
        private IInputService _inputService;
        
        private Filter _cars;
        private AspectFactory<CarSetupAspect> _carSetupAspect;
        
        private Stash<MotorTorqueComponent> _motorTorqueStash;
        private Stash<SteeringAngleComponent> _steeringAngleStash;
        
        private Stash<WheelInfoComponent> _wheelInfoStash;

        [Inject]
        public void Construct(IInputService inputService)
        {
            _inputService = inputService;
        }
        
        public void OnAwake()
        {
            _cars = World.Filter.Extend<CarSetupAspect>().Build();
            _carSetupAspect = World.GetAspectFactory<CarSetupAspect>();
            
            _cars = World.Filter.With<WheelInfoComponent>().Build();
            _wheelInfoStash = World.GetStash<WheelInfoComponent>();
            
            // works thanks aspect, mb
            //_cars = World.Filter
            //    .With<MotorTorqueComponent>()
            //    .With<SteeringAngleComponent>()
            //    .With<WheelInfoComponent>()
            //    .Build();
            //
            //_motorTorqueStash = World.GetStash<MotorTorqueComponent>();
            //_steeringAngleStash = World.GetStash<SteeringAngleComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var car in _cars)
            {
                //ref var motorTorque = ref _motorTorqueStash.Get(car);
                //ref var steeringAngle = ref _steeringAngleStash.Get(car);
                
                ref var wheelInfo = ref _wheelInfoStash.Get(car);
                
                var carSetupAspect = _carSetupAspect.Get(car);
                ref var motorTorque = ref carSetupAspect.MotorTorque;
                ref var steeringAngle = ref carSetupAspect.SteeringAngle;
                
                _inputService.ApplySpeed_PreFin(motorTorque.Value, steeringAngle.Value, wheelInfo.WheelInfo);
                
                Debug.Log($"AAA: info system: {motorTorque.Value}/ {steeringAngle.Value}/ {wheelInfo.FrontWheels.Motor}/ {wheelInfo.FrontWheels.Steering}");
            }
        }

        public void Dispose() { }
    }
}