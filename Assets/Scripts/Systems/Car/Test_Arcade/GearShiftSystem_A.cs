using Components;
using Configs.Impl;
using Scellecs.Morpeh;
using Zenject;

namespace Systems.Car.Test_Arcade
{
    public sealed class GearShiftSystem_A : IFixedSystem
    {
        [Inject] public World World { get; set; }
        [Inject] private CarParameters _params;

        private Filter _cars;
        private Stash<GearComponent> _gearStash;
        private Stash<SpeedComponent> _speedStash;
        private Stash<BackSpeedComponent> _backSpeedStash;
        private Stash<VerticalInputComponent> _vertStash;

        public void OnAwake()
        {
            _cars = World.Filter
                .With<GearComponent>()
                .With<SpeedComponent>()
                .With<BackSpeedComponent>()
                .With<VerticalInputComponent>()
                .Build();

            _gearStash      = World.GetStash<GearComponent>();
            _speedStash     = World.GetStash<SpeedComponent>();
            _backSpeedStash = World.GetStash<BackSpeedComponent>();
            _vertStash      = World.GetStash<VerticalInputComponent>();
        }

        public void OnUpdate(float deltaTime)
        {
            
        }

        public void Dispose() { }
    }
}