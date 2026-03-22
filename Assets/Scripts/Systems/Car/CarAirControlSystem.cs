using System.Collections.Generic;
using Components;
using Configs.Impl;
using Data.HelperClass;
using Scellecs.Morpeh;
using UnityEngine;
using Zenject;

namespace Systems.Car
{
    public sealed class CarAirControlSystem : IFixedSystem
    {
        [Inject] public World World { get; set; }
        [Inject] private CarSelectionParameters _carSelectionParameters;

        private static RaycastHit[] _raycastHits = new RaycastHit[8];

        private Filter _cars;
        private Stash<RigidbodyComponent> _rigidbodyStash;
        private Stash<TransformComponent> _transformStash;
        private Stash<WheelInfoComponent> _wheelInfoStash;

        public void OnAwake()
        {
            _cars = World.Filter
                .With<RigidbodyComponent>()
                .With<TransformComponent>()
                .With<WheelInfoComponent>()
                .Build();

            _rigidbodyStash = World.GetStash<RigidbodyComponent>();
            _transformStash = World.GetStash<TransformComponent>();
            _wheelInfoStash = World.GetStash<WheelInfoComponent>();
        }

        public void OnUpdate(float deltaTime)
        {
            var movementParameters = _carSelectionParameters != null ? _carSelectionParameters.MovementParameters : null;
            
            if (movementParameters == null)
                return;

            var airControl = movementParameters.AirControl;

            foreach (var car in _cars)
            {
                var carRigidbody = _rigidbodyStash.Get(car).Value;
                var carTransform = _transformStash.Get(car).Value;
                var wheelInfo = _wheelInfoStash.Get(car).WheelInfo;

                if (carRigidbody == null || carTransform == null || wheelInfo == null)
                    continue;

                GetWheelGroundStats(wheelInfo, out var totalWheels, out var groundedWheels);
                
                var liftedWheels = totalWheels - groundedWheels;
                if (totalWheels == 0 || liftedWheels < 2)
                    continue;

                var velocity = carRigidbody.velocity;
                if (velocity.y > 0f)
                {
                    velocity.y = 0f;
                    carRigidbody.velocity = velocity;
                }

                var tryGetGroundDistance = TryGetGroundDistance(
                    carRigidbody, carTransform, airControl.GroundMask, airControl.MaxAirborneHeight,
                    out var groundPoint, out var distance);
                
                if (airControl.MaxAirborneHeight > 0f && tryGetGroundDistance && distance > airControl.MaxAirborneHeight)
                {
                    var pos = carRigidbody.position;
                    pos.y = groundPoint.y + airControl.MaxAirborneHeight;
                    carRigidbody.position = pos;
                }

                ApplyUprightStabilization(carRigidbody, carTransform, airControl);
            }
        }

        private static void GetWheelGroundStats(
            IReadOnlyList<WheelInfoSetup> wheelInfo,
            out int totalWheels,
            out int groundedWheels
        )
        {
            totalWheels = 0;
            groundedWheels = 0;

            for (var i = 0; i < wheelInfo.Count; i++)
            {
                var info = wheelInfo[i];
                if (info == null)
                    continue;

                CountGroundedWheel(info.LeftWheel, ref totalWheels, ref groundedWheels);
                CountGroundedWheel(info.RightWheel, ref totalWheels, ref groundedWheels);
            }
        }

        private static void CountGroundedWheel(WheelCollider wheel, ref int totalWheels, ref int groundedWheels)
        {
            if (wheel == null)
                return;

            totalWheels++;
            if (wheel.isGrounded)
                groundedWheels++;
        }

        private static bool TryGetGroundDistance(
            Rigidbody rigidbody,
            Transform transform,
            LayerMask groundMask,
            float maxAirborneHeight,
            out Vector3 groundPoint,
            out float distance
        )
        {
            groundPoint = Vector3.zero;
            distance = 0f;

            var origin = transform.position;
            var rayDistance = Mathf.Max(1f, maxAirborneHeight + 5f);
            var size = Physics.RaycastNonAlloc(
                origin, Vector3.down, _raycastHits, rayDistance, groundMask, QueryTriggerInteraction.Ignore);

            while (size == _raycastHits.Length)
            {
                _raycastHits = new RaycastHit[_raycastHits.Length * 2];
                size = Physics.RaycastNonAlloc(
                    origin, Vector3.down, _raycastHits, rayDistance, groundMask, QueryTriggerInteraction.Ignore);
            }

            if (size == 0)
                return false;

            var bestDistance = float.PositiveInfinity;
            var bestHit = new RaycastHit();

            for (var i = 0; i < size; i++)
            {
                var hit = _raycastHits[i];
                var hitCollider = hit.collider;
                if (hitCollider == null)
                    continue;

                if (hit.rigidbody == rigidbody || hitCollider.transform.IsChildOf(transform))
                    continue;

                if (hit.distance < bestDistance)
                {
                    bestDistance = hit.distance;
                    bestHit = hit;
                }
            }

            if (float.IsPositiveInfinity(bestDistance))
                return false;

            groundPoint = bestHit.point;
            distance = bestDistance;
            return true;
        }

        private static void ApplyUprightStabilization(Rigidbody rigidbody, Transform transform, CarMovementAirControlSetup airControl)
        {
            if (airControl.UprightTorque <= 0f)
                return;

            var angle = Vector3.Angle(transform.up, Vector3.up);
            if (angle < airControl.UprightStartAngleDeg)
                return;

            var torqueAxis = Vector3.Cross(transform.up, Vector3.up);
            rigidbody.AddTorque(torqueAxis * airControl.UprightTorque, ForceMode.Acceleration);

            if (airControl.UprightDamping > 0f)
                rigidbody.AddTorque(-rigidbody.angularVelocity * airControl.UprightDamping, ForceMode.Acceleration);
        }

        public void Dispose() { }
    }
}