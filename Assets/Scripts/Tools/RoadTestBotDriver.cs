#if ROAD_TEST_BOT_ENABLED
using System.Collections.Generic;
using Data.HelperClass;
using Helpers.Car.Impl;
using UnityEngine;

namespace Tools
{
    [DisallowMultipleComponent]
    public sealed class RoadTestBotDriver : MonoBehaviour
    {
        private const float KmhToMps = 1f / 3.6f;

        private readonly List<WheelInfoSetup> _wheelInfos = new List<WheelInfoSetup>();

        private RoadTestBotRoute _route;
        private Rigidbody _rigidbody;
        private Transform _carTransform;
        private float _progressT;
        private bool _isRunning;

        public void Initialize(RoadTestBotRoute route)
        {
            _route = route;

            CacheWheels();
            var carView = GetComponentInChildren<CarView>();
            _rigidbody = carView != null && carView.CarRigidbody != null
                ? carView.CarRigidbody
                : GetComponentInChildren<Rigidbody>();
            _carTransform = carView != null ? carView.CarTransform : (_rigidbody != null ? _rigidbody.transform : transform);

            if (_rigidbody == null)
                Debug.LogWarning("RoadTestBotDriver: Rigidbody was not found on bot prefab.", this);
            if (_wheelInfos.Count == 0)
                Debug.LogWarning("RoadTestBotDriver: WheelColliders were not found on bot prefab.", this);

            PlaceAtStart();
            _isRunning = _route.AutoStart;
        }

        public void StartRun()
        {
            _isRunning = true;
        }

        public void StopRun()
        {
            _isRunning = false;
            ApplyBrake(1f);
            ApplyMotor(0f);
        }

        public void RestartRun()
        {
            PlaceAtStart();
            _isRunning = true;
        }

        private void FixedUpdate()
        {
            if (!_isRunning || _route == null || !_route.HasRoute())
                return;

            if (_rigidbody == null || _wheelInfos.Count == 0)
                return;

            UpdateProgress();

            if (HasReachedEnd())
            {
                if (_route.Loop)
                {
                    PlaceAtStart();
                }
                else
                {
                    StopRun();
                }

                return;
            }

            var targetPoint = _route.GetWorldPointAhead(_progressT, _route.LookAheadMeters);
            targetPoint = _route.ApplyLateralOffset(targetPoint, _progressT);

            var toTarget = Vector3.ProjectOnPlane(targetPoint - _carTransform.position, Vector3.up);
            var forward = Vector3.ProjectOnPlane(_carTransform.forward, Vector3.up);

            var steerInput = 0f;
            if (toTarget.sqrMagnitude > 0.01f && forward.sqrMagnitude > 0.01f)
            {
                var signedAngle = Vector3.SignedAngle(forward.normalized, toTarget.normalized, Vector3.up);
                steerInput = Mathf.Clamp(signedAngle / Mathf.Max(1f, _route.MaxSteerAngle), -1f, 1f);
            }

            ApplySteering(steerInput);

            var speedKmh = _rigidbody.velocity.magnitude / KmhToMps;
            var speedError = _route.TargetSpeedKmh - speedKmh;
            var throttle = Mathf.Clamp01(speedError / Mathf.Max(1f, _route.TargetSpeedKmh * 0.35f));
            var brake = speedError < -_route.BrakeSpeedToleranceKmh ? Mathf.Clamp01(-speedError / 20f) : 0f;

            ApplyBrake(brake);
            ApplyMotor(brake > 0f ? 0f : throttle);
            UpdateWheelVisuals();
        }

        private void CacheWheels()
        {
            _wheelInfos.Clear();

            var carView = GetComponentInChildren<CarView>();
            if (carView != null && carView.CarWheelInfos != null)
            {
                for (var i = 0; i < carView.CarWheelInfos.Count; i++)
                {
                    if (carView.CarWheelInfos[i] != null)
                        _wheelInfos.Add(carView.CarWheelInfos[i]);
                }
            }

            if (_wheelInfos.Count > 0)
                return;

            var wheelColliders = GetComponentsInChildren<WheelCollider>();
            for (var i = 0; i < wheelColliders.Length; i += 2)
            {
                var leftWheel = wheelColliders[i];
                var rightWheel = i + 1 < wheelColliders.Length ? wheelColliders[i + 1] : wheelColliders[i];
                _wheelInfos.Add(new WheelInfoSetup
                {
                    LeftWheel = leftWheel,
                    RightWheel = rightWheel,
                    Motor = true,
                    Steering = i == 0
                });
            }
        }

        private void PlaceAtStart()
        {
            if (_route == null || !_route.HasRoute())
                return;

            _progressT = _route.DriveStartT;

            var position = _route.ApplyLateralOffset(_route.GetWorldPoint(_progressT), _progressT);
            var rotation = _route.GetWorldRotation(_progressT, transform.forward);

            if (_rigidbody != null)
            {
                _rigidbody.velocity = Vector3.zero;
                _rigidbody.angularVelocity = Vector3.zero;
                _rigidbody.position = position + Vector3.up * _route.SpawnHeightOffset;
                _rigidbody.rotation = rotation;
            }

            _carTransform.SetPositionAndRotation(position + Vector3.up * _route.SpawnHeightOffset, rotation);
        }

        private void UpdateProgress()
        {
            _progressT = _route.GetNearestT(_carTransform.position, _progressT);
        }

        private bool HasReachedEnd()
        {
            return _route.HasPassedDriveEnd(_carTransform.position, _progressT);
        }

        private void ApplySteering(float input)
        {
            var targetAngle = input * _route.MaxSteerAngle;
            var maxDelta = _route.SteeringSpeed * Time.fixedDeltaTime;

            for (var i = 0; i < _wheelInfos.Count; i++)
            {
                var info = _wheelInfos[i];
                if (info == null || !info.Steering)
                    continue;

                ApplySteerToWheel(info.LeftWheel, targetAngle, maxDelta);
                ApplySteerToWheel(info.RightWheel, targetAngle, maxDelta);
            }
        }

        private static void ApplySteerToWheel(WheelCollider wheel, float targetAngle, float maxDelta)
        {
            if (wheel == null)
                return;

            wheel.steerAngle = Mathf.MoveTowards(wheel.steerAngle, targetAngle, maxDelta);
        }

        private void ApplyMotor(float throttle)
        {
            var torque = Mathf.Max(0f, _route.MotorTorque) * Mathf.Clamp01(throttle);

            for (var i = 0; i < _wheelInfos.Count; i++)
            {
                var info = _wheelInfos[i];
                if (info == null || !info.Motor)
                    continue;

                ApplyMotorToWheel(info.LeftWheel, torque);
                ApplyMotorToWheel(info.RightWheel, torque);
            }
        }

        private static void ApplyMotorToWheel(WheelCollider wheel, float torque)
        {
            if (wheel == null)
                return;

            wheel.motorTorque = torque;
        }

        private void ApplyBrake(float brake)
        {
            var brakeTorque = Mathf.Max(0f, _route.BrakeTorque) * Mathf.Clamp01(brake);

            for (var i = 0; i < _wheelInfos.Count; i++)
            {
                var info = _wheelInfos[i];
                if (info == null)
                    continue;

                ApplyBrakeToWheel(info.LeftWheel, brakeTorque);
                ApplyBrakeToWheel(info.RightWheel, brakeTorque);
            }
        }

        private static void ApplyBrakeToWheel(WheelCollider wheel, float brakeTorque)
        {
            if (wheel == null)
                return;

            wheel.brakeTorque = brakeTorque;
        }

        private void UpdateWheelVisuals()
        {
            for (var i = 0; i < _wheelInfos.Count; i++)
            {
                var info = _wheelInfos[i];
                if (info == null)
                    continue;

                ApplyWheelVisual(info.LeftWheel, info.LeftVisual);
                ApplyWheelVisual(info.RightWheel, info.RightVisual);
            }
        }

        private static void ApplyWheelVisual(WheelCollider wheel, Transform visual)
        {
            if (wheel == null || visual == null)
                return;

            wheel.GetWorldPose(out var position, out var rotation);
            visual.SetPositionAndRotation(position, rotation);
        }
    }
}
#endif
