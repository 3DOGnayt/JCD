using Components;
using Scellecs.Morpeh;
using UnityEngine;

namespace Systems.Car
{
    public class PhysicsSpeedSystem_Test : IFixedSystem 
    {
        public World World { get; set; }
        private Filter _f;
        private Stash<RigidbodyComponent> _rb;
        private Stash<SpeedComponent> _sp;

        public void OnAwake() 
        {
            _f = World.Filter.With<RigidbodyComponent>().With<SpeedComponent>().Build();
            _rb = World.GetStash<RigidbodyComponent>(); _sp = World.GetStash<SpeedComponent>();
        }
        public void OnUpdate(float dt)
        {
            foreach (var e in _f)
            {
                ref var rb = ref _rb.Get(e);
                ref var sp = ref _sp.Get(e);
                sp.Value = rb.Value.velocity.magnitude * 3.6f;
            }
        }
        public void Dispose() {}
    }
}