using Components;
using Configs.Impl;
using Scellecs.Morpeh;
using Zenject;

namespace Systems.Car.Test_Arcade
{
    public class GearShiftSystem_A : IFixedSystem
    {
        [Inject] public World World { get; set; }
        [Inject] private CarMovementParameters _carMovementParameters;

        private Filter _carFilter;
        private Stash<EngineRpmComponent> _engineRpmStash;
        private Stash<GearboxComponent> _gearStash;
        private Stash<SpeedComponent> _speedStash;
        private Stash<VerticalInputComponent> _verticalStash;

        public void OnAwake()
        {
            _carFilter = World.Filter
                .With<EngineRpmComponent>()
                .With<GearboxComponent>()
                .With<SpeedComponent>()
                .With<VerticalInputComponent>()
                .Build();

            _engineRpmStash = World.GetStash<EngineRpmComponent>();
            _gearStash      = World.GetStash<GearboxComponent>();
            _speedStash     = World.GetStash<SpeedComponent>();
            _verticalStash  = World.GetStash<VerticalInputComponent>();
        }

        public void OnUpdate(float deltaTime)
        {
            
        }

        public void Dispose() { }
    }
}
