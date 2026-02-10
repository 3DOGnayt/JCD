using System;
using System.Collections.Generic;
using Data;
using UniRx;
using UnityEngine;

namespace Services.Impl
{
    public class InputService : IInputService, IDisposable
    {
        private readonly ILoadingService _loadingService;
        private readonly IDisposable _inputEnabledSubscription;
        private bool _inputEnabled = true;

        public InputService(ILoadingService loadingService)
        {
            _loadingService = loadingService;
            _inputEnabledSubscription = _loadingService.InputEnabledStream.Subscribe(SetInputEnabled);
        }

        public void ApplyHorizontalMove(float targetAngle, float steeringSpeed, List<WheelInfoSetup> wheelInfos)
        {
            if (!_inputEnabled)
                return;

            foreach (var info in wheelInfos)
            {
                if (!info.Steering)
                    continue;

                var left = Mathf.MoveTowards(
                    info.LeftWheel.steerAngle,
                    targetAngle,
                    steeringSpeed * Time.fixedDeltaTime);

                var right = Mathf.MoveTowards(
                    info.RightWheel.steerAngle,
                    targetAngle,
                    steeringSpeed * Time.fixedDeltaTime);
                
                info.LeftWheel.steerAngle = left;
                info.RightWheel.steerAngle = right;

                ApplyLocalPositionToVisuals(info.LeftWheel, info.LeftVisual);
                ApplyLocalPositionToVisuals(info.RightWheel, info.RightVisual);
            }
        }

        public void ApplyVerticalMove(float currentMotorTorque, float input, List<WheelInfoSetup> wheelInfos)
        {
            if (!_inputEnabled)
                return;

            var torque = currentMotorTorque * input;
            //var torque = currentMotorTorque * Mathf.Sign(input);
            
            foreach (var info in wheelInfos)
            {
                if (!info.Motor) 
                    continue;
                
                info.LeftWheel.motorTorque = torque;
                info.RightWheel.motorTorque = torque;
                
                ApplyLocalPositionToVisuals(info.LeftWheel, info.LeftVisual);
                ApplyLocalPositionToVisuals(info.RightWheel, info.RightVisual);
            }
        }

        private void ApplyLocalPositionToVisuals(WheelCollider wheelCollider, Transform visualWheel)
        {
            Vector3 position;
            Quaternion rotation;
            wheelCollider.GetWorldPose(out position, out rotation);

            visualWheel.position = position;
            visualWheel.rotation = rotation;
        }

        private void SetInputEnabled(bool isEnabled)
        {
            _inputEnabled = isEnabled;
        }

        public void Dispose()
        {
            _inputEnabledSubscription?.Dispose();
        }
    }
}