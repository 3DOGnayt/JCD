using Components;
using Configs.Impl;
using Scellecs.Morpeh;
using UnityEngine;
using Zenject;

namespace Systems.Car.Main
{
    public sealed class InputSystem : ISystem
    {
        [Inject] public World World { get; set;}
        [Inject] private CarSelectionParameters _carSelectionParameters;
        
        private Filter _cars;
        private Stash<WheelInfoComponent> _wheelInfoStash;
        
        private Stash<VerticalInputComponent> _verticalStash;
        private Stash<HorizontalInputComponent> _horizontalStash;
        private Stash<HandbrakeInputComponent> _handbrakeStash;
        private Stash<ShiftUpInputComponent> _shiftUpStash;
        private Stash<ShiftDownInputComponent> _shiftDownStash;
        
        public void OnAwake()
        {
            _cars = World.Filter.Extend<CarSetupAspect>().Build();
            
            _cars = World.Filter
                .With<PlayerTagComponent>()
                .With<WheelInfoComponent>()
                .With<VerticalInputComponent>()
                .With<HorizontalInputComponent>()
                .With<ShiftUpInputComponent>()
                .With<ShiftDownInputComponent>()
                .Build();
            
            _verticalStash = World.GetStash<VerticalInputComponent>();
            _horizontalStash = World.GetStash<HorizontalInputComponent>();
            _handbrakeStash = World.GetStash<HandbrakeInputComponent>();
            _shiftUpStash = World.GetStash<ShiftUpInputComponent>();
            _shiftDownStash = World.GetStash<ShiftDownInputComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            var touge = _carSelectionParameters != null && _carSelectionParameters.MovementParameters != null
                ? _carSelectionParameters.MovementParameters.Touge
                : null;
            var useTouge = touge != null && touge.UseTougeHybridControl;
            
            foreach (var car in _cars)
            {
                ref var vertical = ref _verticalStash.Get(car);
                ref var horizontal = ref _horizontalStash.Get(car);
                ref var handBrake = ref _handbrakeStash.Get(car).Value;
                ref var shiftUp = ref _shiftUpStash.Get(car).Value;
                ref var shiftDown = ref _shiftDownStash.Get(car).Value;
                
                var vert = Input.GetAxis("Vertical");
                var hor = Input.GetAxis("Horizontal");

                vertical.Value = Mathf.Abs(vert) < 0.01f ? 0 : vert;
                horizontal.Value = Mathf.Abs(hor) < 0.01f ? 0 : hor;

                handBrake = !useTouge || !touge.DisableHandbrakeDrift
                    ? Input.GetKey(KeyCode.Space)
                    : false;

                shiftUp = useTouge && Input.GetKeyDown(touge.ShiftUpKey);
                shiftDown = useTouge && Input.GetKeyDown(touge.ShiftDownKey);
            }
        }

        public void Dispose() { }
    }
}
