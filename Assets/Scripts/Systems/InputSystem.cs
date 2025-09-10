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
        private Stash<MotorTorqueComponent> _motorTorqueStash;
        private Stash<SteeringAngleComponent> _steeringAngleStash;

        [Inject]
        public void Construct(IInputService inputService)
        {
            _inputService = inputService;
        }
        
        public void OnAwake()
        {
            _cars = World.Filter
                .With<MotorTorqueComponent>()
                .With<SteeringAngleComponent>()
                .Build();
            
            _motorTorqueStash = World.GetStash<MotorTorqueComponent>();
            _steeringAngleStash = World.GetStash<SteeringAngleComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var car in _cars)
            {
                
                //_inputService.ApplySpeed_Test(_carSetup, _wheelInfos); 
                
                
                Debug.Log($"AAA: ");
            }
        }

        public void Dispose() { }
    }
}