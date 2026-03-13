using System;
using Components;
using Configs.Impl;
using Scellecs.Morpeh;
using Services;
using UniRx;
using UnityEngine;
using Zenject;

namespace Systems.Car
{
    public sealed class PhysicsSpeedSystem : IFixedSystem
    {
        [Inject] public World World { get; set; }
        [Inject] private CarSelectionParameters _carSelectionParameters;
        [Inject] private IEventService _eventService;

        private Filter _cars;
        private AspectFactory<CarSetupAspect> _carAspectFactory;
        private Stash<RigidbodyComponent> _rigidbodyStash;
        private Stash<TransformComponent> _transformStash;
        private Stash<VerticalInputComponent> _vertStash;
        private Stash<HandbrakeInputComponent> _handbrakeStash;

        private float _assistForwardSpeedMps;
        private bool _inputEnabled = true;
        private IDisposable _inputEnabledSubscription;

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

            if (_eventService != null)
                _inputEnabledSubscription = _eventService.InputEnabledStream.Subscribe(isEnabled => _inputEnabled = isEnabled);
        }

        public void OnUpdate(float deltaTime)
        {
            var carParameters = _carSelectionParameters != null ? _carSelectionParameters.SelectedCarParameters : null;
            if (carParameters == null)
                return;

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

                velocity = ApplyArcadeAssist(velocity, input, forward, deltaTime, carParameters);
                
                rb.velocity = velocity;

                ApplySleepIfStopped(rb, input, handbrake, carParameters);

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
            float deltaTime,
            CarParameters carParameters)
        {
            var movementParameters = carParameters.MovementParameters;
            if (!movementParameters.UseArcadeAssist)
            {
                _assistForwardSpeedMps = Vector3.Dot(velocity, forward);
                return velocity;
            }

            var forwardSpeed = Vector3.Dot(velocity, forward);
            var absForward = Mathf.Abs(forwardSpeed);
            var minSpeedMps = movementParameters.ArcadeAssistMinSpeedKmh / 3.6f;

            var wantForward = verticalInput > 0.01f;
            var movingForward = forwardSpeed > 0.01f;
            var driftAssistMultiplier = Mathf.Max(1f, movementParameters.DriftAssistForwardSpeedMultiplier);
            var isDrifting = IsDrifting(velocity, forward, carParameters);

            if (isDrifting && !movementParameters.UseArcadeAssistInDrift)
            {
                _assistForwardSpeedMps = forwardSpeed;
                return velocity;
            }

            if (!wantForward || !movingForward || absForward < minSpeedMps)
            {
                _assistForwardSpeedMps = forwardSpeed;
                return velocity;
            }

            if (Mathf.Abs(_assistForwardSpeedMps) <= 0.01f)
                _assistForwardSpeedMps = forwardSpeed;

            if (forwardSpeed < _assistForwardSpeedMps)
            {
                var t = 1f - Mathf.Exp(-movementParameters.ArcadeAssistLerpSpeed * deltaTime);
                var targetForward = Mathf.Lerp(
                    forwardSpeed,
                    _assistForwardSpeedMps * (isDrifting ? driftAssistMultiplier : 1f),
                    t);

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

        private bool IsDrifting(Vector3 velocity, Vector3 forward, CarParameters carParameters)
        {
            var helpers = carParameters.MovementParameters.HelpersSetup;
            if (helpers == null)
                return false;

            var speedKmh = velocity.magnitude * 3.6f;
            if (speedKmh < helpers.MinDriftSpeedKmh)
                return false;

            var slipAngle = Vector3.Angle(forward, velocity);
            return slipAngle > helpers.SlipAngleThresholdDeg;
        }
        
        private void ApplySleepIfStopped(Rigidbody rb, float verticalInput, bool handbrake, CarParameters carParameters)
        {
            if (!_inputEnabled)
                return;

            if (Mathf.Abs(verticalInput) > 0.01f && !handbrake)
                return;

            var v = rb.velocity;
            var av = rb.angularVelocity;

            var systemHelpers = carParameters.MovementParameters.HelpersSetup;
            var sleepSpeedThresholdMps = systemHelpers.SleepSpeedThresholdMps * systemHelpers.SleepSpeedThresholdMps;
            var sleepAngularSpeedThreshold = systemHelpers.SleepAngularSpeedThreshold * systemHelpers.SleepAngularSpeedThreshold;
            
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

        public void Dispose()
        {
            _inputEnabledSubscription?.Dispose();
        }
    }
}