using Components;
using Scellecs.Morpeh;
using UnityEngine;

namespace Systems.Car
{
    public class PhysicsSpeedSystem : IFixedSystem
    {
        public World World { get; set; }

        private Filter _filter;
        private Stash<RigidbodyComponent> _rbStash;
        private Stash<SpeedComponent> _speedStash;

        // Константа сглаживания: чем меньше τ, тем быстрее реакция (0.15–0.35 сек обычно ок)
        private const float TauSeconds = 0.25f;

        public void OnAwake()
        {
            _filter = World.Filter.With<RigidbodyComponent>().With<SpeedComponent>().Build();
            _rbStash = World.GetStash<RigidbodyComponent>();
            _speedStash = World.GetStash<SpeedComponent>();
        }

        public void OnUpdate(float deltaTime)
        {
            // коэффициент экспоненциального сглаживания
            float alpha = 1f - Mathf.Exp(-deltaTime / Mathf.Max(0.0001f, TauSeconds));

            foreach (var e in _filter)
            {
                ref var rb = ref _rbStash.Get(e);
                ref var sp = ref _speedStash.Get(e);

                float rawKmh = rb.Value.velocity.magnitude * 3.6f;
                sp.Value += (rawKmh - sp.Value) * alpha; // EMA
            }
        }

        public void Dispose() { }
    }
}