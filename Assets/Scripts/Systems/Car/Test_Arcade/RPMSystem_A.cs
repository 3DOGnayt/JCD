using Components;
using Configs.Impl;
using Scellecs.Morpeh;
using Zenject;

namespace Systems.Car.Test_Arcade
{
    public sealed class RPMSystem_A : IFixedSystem
    {
        [Inject] public World World { get; set; }
        [Inject] private CarMovementParameters _carMovementParameters;

        private Filter _carFilter;
        private Stash<EngineRpmComponent> _rpmStash;
        private Stash<VerticalInputComponent> _vertStash;
        private Stash<GearboxComponent> _gearStash;

        public void OnAwake()
        {
            _carFilter = World.Filter
                .With<EngineRpmComponent>()
                .With<VerticalInputComponent>()
                .With<GearboxComponent>()
                .Build();

            _rpmStash  = World.GetStash<EngineRpmComponent>();
            _vertStash = World.GetStash<VerticalInputComponent>();
            _gearStash = World.GetStash<GearboxComponent>();
        }

        public void OnUpdate(float deltaTime)
        {
            
        }

        public void Dispose() { }
    }
}
