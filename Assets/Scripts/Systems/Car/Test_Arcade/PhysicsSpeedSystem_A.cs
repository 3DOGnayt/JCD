using Components;
using Scellecs.Morpeh;
using UnityEngine;
using Zenject;

namespace Systems.Car.Test_Arcade
{
    public sealed class PhysicsSpeedSystem_A : IFixedSystem
    {
        [Inject] public World World { get; set; }

        private Filter _cars;
        private AspectFactory<CarSetupAspect> _carAspectFactory;
        private Stash<RigidbodyComponent> _rigidbodyStash;
        private Stash<TransformComponent> _transformStash;

        public void OnAwake()
        {
            _cars = World.Filter
                .Extend<CarSetupAspect>()          // Speed + BackSpeed и прочее
                .With<RigidbodyComponent>()
                .With<TransformComponent>()
                .Build();

            _carAspectFactory = World.GetAspectFactory<CarSetupAspect>();
            _rigidbodyStash   = World.GetStash<RigidbodyComponent>();
            _transformStash   = World.GetStash<TransformComponent>();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var car in _cars)
            {
                var aspect = _carAspectFactory.Get(car);
                ref var speed     = ref aspect.Speed;
                ref var backSpeed = ref aspect.BackSpeed;

                var rbComp  = _rigidbodyStash.Get(car);
                var trComp  = _transformStash.Get(car);

                if (rbComp.Value == null || trComp.Value == null)
                    continue;

                var vel     = rbComp.Value.velocity;
                var forward = trComp.Value.forward;

                var forwardSpeedMps = Vector3.Dot(vel, forward);      // со знаком
                var speedKmh        = Mathf.Abs(forwardSpeedMps) * 3.6f;

                if (forwardSpeedMps >= 0f)
                {
                    speed.Value     = speedKmh;
                    backSpeed.Value = 0f;
                }
                else
                {
                    speed.Value     = 0f;
                    backSpeed.Value = speedKmh;
                }
            }
        }

        public void Dispose() { }
    }
}