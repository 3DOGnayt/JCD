using Components;
using Configs.Impl;
using Scellecs.Morpeh;
using Zenject;

namespace Systems.Car.Test_Manual
{
    public sealed class RPMSystem_M : IFixedSystem
    {
        [Inject] public World World { get; set; }
        [Inject] private CarParameters _carParameters;

        private Filter _carFilter;
        private Stash<EngineRpmComponent> _rpmStash;
        private Stash<VerticalInputComponent> _vertStash;
        private Stash<GearComponent> _gearStash;

        public void OnAwake()
        {
            _carFilter = World.Filter
                .With<EngineRpmComponent>()
                .With<VerticalInputComponent>()
                .With<GearComponent>()
                .Build();

            _rpmStash  = World.GetStash<EngineRpmComponent>();
            _vertStash = World.GetStash<VerticalInputComponent>();
            _gearStash = World.GetStash<GearComponent>();
        }

        public void OnUpdate(float deltaTime)
        {
            
        }

        public void Dispose() { }
    }
}
