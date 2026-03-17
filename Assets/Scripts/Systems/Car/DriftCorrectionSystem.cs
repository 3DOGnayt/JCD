using Components;
using Configs.Impl;
using Scellecs.Morpeh;
using UnityEngine;
using Zenject;

namespace Systems.Car
{
    public sealed class DriftCorrectionSystem : IFixedSystem
    {
        [Inject] public World World { get; set; }
        [Inject] private CarSelectionParameters _carSelectionParameters;

        private Filter _cars;
        private Stash<RigidbodyComponent> _rigidbodyStash;
        private Stash<TransformComponent> _transformStash;
        private Stash<HorizontalInputComponent> _horizontalInputStash;

        private struct DriftCorrectionSettings
        {
            public float MinSpeedKmh;
            public float MinSlipAngleDeg;
            public float AlignTorque;
            public float AlignDamping;
            public float InputDeadZone;
            public bool RequireCounterSteer;
        }

        public void OnAwake()
        {
            _cars = World.Filter
                .With<RigidbodyComponent>()
                .With<TransformComponent>()
                .With<HorizontalInputComponent>()
                .Build();

            _rigidbodyStash = World.GetStash<RigidbodyComponent>();
            _transformStash = World.GetStash<TransformComponent>();
            _horizontalInputStash = World.GetStash<HorizontalInputComponent>();
        }

        public void OnUpdate(float deltaTime)
        {
            if (!TryGetSettings(out var settings))
                return;

            foreach (var car in _cars)
                ApplyDriftCorrection(car, settings);
        }

        private bool TryGetSettings(out DriftCorrectionSettings settings)
        {
            settings = default;

            var carParameters = _carSelectionParameters != null ? _carSelectionParameters.SelectedCarParameters : null;
            if (carParameters == null)
                return false;

            var movement = carParameters.MovementParameters;
            var velocityAlign = movement.VelocityAlign;
            if (!velocityAlign.UseVelocityAlign)
                return false;

            var helpers = movement.HelpersSetup;
            if (helpers == null)
                return false;

            var alignTorque = velocityAlign.VelocityAlignTorque;
            if (alignTorque <= 0f)
                return false;

            settings.MinSpeedKmh = velocityAlign.VelocityAlignMinSpeedKmh;
            settings.MinSlipAngleDeg = velocityAlign.VelocityAlignMinSlipAngleDeg;
            settings.AlignTorque = alignTorque;
            settings.AlignDamping = velocityAlign.VelocityAlignDamping;
            settings.InputDeadZone = helpers.InputDeadZone;
            settings.RequireCounterSteer = velocityAlign.VelocityAlignRequireCounterSteer;
            return true;
        }

        private void ApplyDriftCorrection(Entity car, DriftCorrectionSettings settings)
        {
            var rigidbody = _rigidbodyStash.Get(car).Value;
            var transform = _transformStash.Get(car).Value;
            if (rigidbody == null || transform == null)
                return;

            if (!TryGetFlatVelocityDirection(rigidbody, settings.MinSpeedKmh, out var velocityDirection))
                return;

            if (!TryGetFlatForwardDirection(transform, out var forwardDirection))
                return;

            var slipAngle = Vector3.Angle(forwardDirection, velocityDirection);
            if (slipAngle < settings.MinSlipAngleDeg)
                return;

            if (settings.RequireCounterSteer &&
                !IsCounterSteerInput(car, forwardDirection, velocityDirection, settings.InputDeadZone))
                return;

            ApplyAlignTorque(rigidbody, forwardDirection, velocityDirection, settings.AlignTorque, settings.AlignDamping);
        }

        private static bool TryGetFlatVelocityDirection(
            Rigidbody rigidbody,
            float minSpeedKmh,
            out Vector3 velocityDirection
        )
        {
            var velocity = rigidbody.velocity;
            var flatVelocity = new Vector3(velocity.x, 0f, velocity.z);
            if (flatVelocity.sqrMagnitude < 0.001f)
            {
                velocityDirection = Vector3.zero;
                return false;
            }

            var speedKmh = flatVelocity.magnitude * 3.6f;
            if (speedKmh < minSpeedKmh)
            {
                velocityDirection = Vector3.zero;
                return false;
            }

            velocityDirection = flatVelocity.normalized;
            return true;
        }

        private static bool TryGetFlatForwardDirection(Transform transform, out Vector3 forwardDirection)
        {
            var flatForward = new Vector3(transform.forward.x, 0f, transform.forward.z);
            if (flatForward.sqrMagnitude < 0.001f)
            {
                forwardDirection = Vector3.zero;
                return false;
            }

            forwardDirection = flatForward.normalized;
            return true;
        }

        private bool IsCounterSteerInput(
            Entity car,
            Vector3 forwardDirection,
            Vector3 velocityDirection,
            float inputDeadZone
        )
        {
            var horizontalInput = _horizontalInputStash.Get(car).Value;
            if (Mathf.Abs(horizontalInput) <= inputDeadZone)
                return false;

            var slipSign = Mathf.Sign(Vector3.Cross(forwardDirection, velocityDirection).y);
            return slipSign != 0f && horizontalInput * slipSign > 0f;
        }

        private static void ApplyAlignTorque(
            Rigidbody rigidbody,
            Vector3 forwardDirection,
            Vector3 velocityDirection,
            float alignTorque,
            float alignDamping)
        {
            var axis = Vector3.Cross(forwardDirection, velocityDirection);
            if (axis.sqrMagnitude < 0.0001f)
                return;

            var torque = Vector3.up * axis.y * alignTorque;
            rigidbody.AddTorque(torque, ForceMode.Acceleration);

            if (alignDamping > 0f)
            {
                var angular = rigidbody.angularVelocity;
                rigidbody.AddTorque(Vector3.up * -angular.y * alignDamping, ForceMode.Acceleration);
            }
        }

        public void Dispose() { }
    }
}