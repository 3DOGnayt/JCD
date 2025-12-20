using Components;
using Configs.Impl;
using Scellecs.Morpeh;
using UnityEngine;
using Zenject;

namespace Systems.Car
{
    public sealed class PhysicsSpeedSystem : IFixedSystem
    {
        [Inject] public World World { get; set; }
        [Inject] private CarParameters _carParameters;

        private Filter _cars;
        private AspectFactory<CarSetupAspect> _carAspectFactory;
        private Stash<RigidbodyComponent> _rigidbodyStash;
        private Stash<TransformComponent> _transformStash;
        private Stash<VerticalInputComponent> _vertStash;
        private Stash<HandbrakeInputComponent> _handbrakeStash;

        private float _assistForwardSpeedMps;

        private const float SleepSpeedThresholdMps = 0.05f; // TODO: Refactoring
        private const float SleepAngularSpeedThreshold = 0.05f; // TODO: Refactoring

        public void OnAwake()
        {
            _cars = World.Filter
                .Extend<CarSetupAspect>()
                .With<RigidbodyComponent>()
                .With<TransformComponent>()
                .With<VerticalInputComponent>()
                .Build();

            _carAspectFactory = World.GetAspectFactory<CarSetupAspect>();
            _rigidbodyStash = World.GetStash<RigidbodyComponent>();
            _transformStash = World.GetStash<TransformComponent>();
            _vertStash = World.GetStash<VerticalInputComponent>();
            _handbrakeStash = World.GetStash<HandbrakeInputComponent>();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var car in _cars)
            {
                var aspect = _carAspectFactory.Get(car);
                ref var speed = ref aspect.Speed;
                ref var backSpeed = ref aspect.BackSpeed;

                var rbComp = _rigidbodyStash.Get(car);
                var trComp = _transformStash.Get(car);
                var vert = _vertStash.Get(car);
                var hb = _handbrakeStash.Get(car);

                if (rbComp.Value == null || trComp.Value == null)
                    continue;

                var rb = rbComp.Value;
                var forward = trComp.Value.forward;
                var input = vert.Value;
                var handbrake = hb.Value;

                var velocity = rb.velocity;

                velocity = ApplyArcadeAssist(velocity, input, forward, deltaTime);

                rb.velocity = velocity;

                ApplySleepIfStopped(rb, input, handbrake);

                velocity = rb.velocity;

                var forwardSpeedMps = Vector3.Dot(velocity, forward);
                var forwardKmh = Mathf.Abs(forwardSpeedMps) * 3.6f;

                if (forwardSpeedMps >= 0f)
                {
                    speed.Value = forwardKmh;
                    backSpeed.Value = 0f; // for debug and inside settings
                }
                else
                {
                    speed.Value = 0f;
                    backSpeed.Value = forwardKmh; // for debug and inside settings
                }
            }
        }

        private Vector3 ApplyArcadeAssist(
            Vector3 velocity,
            float verticalInput,
            Vector3 forward,
            float deltaTime)
        {
            if (!_carParameters.MovementParameters.UseArcadeAssist)
            {
                _assistForwardSpeedMps = Vector3.Dot(velocity, forward);
                return velocity;
            }

            var forwardSpeed = Vector3.Dot(velocity, forward);
            var absForward = Mathf.Abs(forwardSpeed);
            var minSpeedMps = _carParameters.MovementParameters.ArcadeAssistMinSpeedKmh / 3.6f;

            var wantForward = verticalInput > 0.01f;
            var movingForward = forwardSpeed > 0.01f;

            if (!wantForward || !movingForward || absForward < minSpeedMps)
            {
                _assistForwardSpeedMps = forwardSpeed;
                return velocity;
            }

            if (Mathf.Abs(_assistForwardSpeedMps) <= 0.01f)
                _assistForwardSpeedMps = forwardSpeed;

            if (forwardSpeed < _assistForwardSpeedMps)
            {
                var t = 1f - Mathf.Exp(-_carParameters.MovementParameters.ArcadeAssistLerpSpeed * deltaTime);
                var targetForward = Mathf.Lerp(forwardSpeed, _assistForwardSpeedMps, t);

                var forwardComponent = forward * forwardSpeed;
                var otherComponent = velocity - forwardComponent;

                var newForwardComponent = forward * targetForward;
                velocity = newForwardComponent + otherComponent;
            }
            else
            {
                _assistForwardSpeedMps = forwardSpeed;
            }

            return velocity;
        }
        
        private void ApplySleepIfStopped(Rigidbody rb, float verticalInput, bool handbrake)
        {
            if (Mathf.Abs(verticalInput) > 0.01f && !handbrake)
                return;

            var v = rb.velocity;
            var av = rb.angularVelocity;

            var sleepSpeedThresholdMps = SleepSpeedThresholdMps * SleepSpeedThresholdMps;
            var sleepAngularSpeedThreshold = SleepAngularSpeedThreshold * SleepAngularSpeedThreshold;
            
            if (v.sqrMagnitude < sleepSpeedThresholdMps && av.sqrMagnitude < sleepAngularSpeedThreshold)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.Sleep();
            }
            else
            {
                if (rb.IsSleeping() && Mathf.Abs(verticalInput) > 0.01f && !handbrake)
                    rb.WakeUp();
            }
        }

        public void Dispose() { }
    }
}