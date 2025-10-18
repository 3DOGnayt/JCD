using Components;
using Scellecs.Morpeh;
using Services;
using Zenject;

namespace Systems
{
    public sealed class InputSystem : ISystem 
    {
        [Inject] public World World { get; set;}
        
        private IInputService _inputService;
        
        private Filter _cars;
        private AspectFactory<CarSetupAspect> _carSetupAspect;
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
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var car in _cars)
            {
                ref var wheelInfo = ref _wheelInfoStash.Get(car);
                
                var carSetupAspect = _carSetupAspect.Get(car);
                ref var motorTorque = ref carSetupAspect.MotorTorque;
                ref var steeringAngle = ref carSetupAspect.SteeringAngle;
                
                _inputService.ApplyMove(motorTorque.Value, steeringAngle.Value, wheelInfo.WheelInfo);
            }
        }

        public void Dispose() { }
    }
}