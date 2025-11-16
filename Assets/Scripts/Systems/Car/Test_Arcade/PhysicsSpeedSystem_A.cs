using Components;
using Scellecs.Morpeh;

namespace Systems.Car.Test_Arcade
{
    public class PhysicsSpeedSystem_A : IFixedSystem 
    {
        public World World { get; set; }
        private Filter _carFilter;
        private Stash<RigidbodyComponent> _rigidbodyStash;
        private Stash<SpeedComponent> _speedStash;

        public void OnAwake() 
        {
            _carFilter = World.Filter.With<RigidbodyComponent>()
                .With<SpeedComponent>().Build();
            
            _rigidbodyStash = World.GetStash<RigidbodyComponent>();
            _speedStash = World.GetStash<SpeedComponent>();
        }
        
        public void OnUpdate(float deltaTime)
        {
            foreach (var e in _carFilter)
            {
                // ref var rb = ref _rigidbodyStash.Get(e);
                // ref var sp = ref _speedStash.Get(e);
                // sp.Value = rb.Value.velocity.magnitude * 3.6f;
            }
        }
        
        public void Dispose() {}
    }
}