using Components;
using Configs.Impl;
using Scellecs.Morpeh;
using Services;
using Zenject;

namespace Systems.Car.Test_Manual
{
    public class WheelDriveSystem_M : IFixedSystem
    {
        [Inject] public World World { get; set; }
        [Inject] private CarMovementParameters _carMovementParameters;
        [Inject] private IInputService _inputService;

        private Filter _filter;
        private Stash<WheelInfoComponent> _wheelStash;
        private Stash<GearComponent> _gearStash;
        private Stash<EngineRpmComponent> _rpmStash;
        private Stash<VerticalInputComponent> _vertStash;
        
        public void OnAwake()
        {
            _filter = World.Filter
                .With<WheelInfoComponent>()
                .With<GearComponent>()
                .With<EngineRpmComponent>()
                .With<VerticalInputComponent>()
                .Build();

            _wheelStash = World.GetStash<WheelInfoComponent>();
            _gearStash = World.GetStash<GearComponent>();
            _rpmStash = World.GetStash<EngineRpmComponent>();
            _vertStash = World.GetStash<VerticalInputComponent>();
        }

        public void OnUpdate(float dt)
        {
            
        }

        public void Dispose()
        {
            
        }
    }
}