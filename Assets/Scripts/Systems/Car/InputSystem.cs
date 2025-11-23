using Components;
using Scellecs.Morpeh;
using UnityEngine;
using Zenject;

namespace Systems.Car
{
    public sealed class InputSystem : ISystem
    {
        [Inject] public World World { get; set;}
        
        private Filter _cars;
        private Stash<WheelInfoComponent> _wheelInfoStash;
        
        private Stash<VerticalInputComponent> _verticalStash;
        private Stash<HorizontalInputComponent> _horizontalStash;
        private Stash<HandbrakeInputComponent> _handbrakeStash;
        
        public void OnAwake()
        {
            _cars = World.Filter.Extend<CarSetupAspect>().Build();
            
            _cars = World.Filter
                .With<WheelInfoComponent>()
                .With<VerticalInputComponent>()
                .With<HorizontalInputComponent>()
                .Build();
            
            _verticalStash = World.GetStash<VerticalInputComponent>();
            _horizontalStash = World.GetStash<HorizontalInputComponent>();
            _handbrakeStash = World.GetStash<HandbrakeInputComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var car in _cars)
            {
                ref var vertical = ref _verticalStash.Get(car);
                ref var horizontal = ref _horizontalStash.Get(car);
                
                var vert = Input.GetAxis("Vertical");
                var hor = Input.GetAxis("Horizontal");

                vertical.Value = Mathf.Abs(vert) < 0.01f ? 0 : vert;
                horizontal.Value = Mathf.Abs(hor) < 0.01f ? 0 : hor;
                
                var handbrake = Input.GetKey(KeyCode.Space);
                _handbrakeStash.Get(car).Value = handbrake;
            }
        }

        public void Dispose() { }
    }
}