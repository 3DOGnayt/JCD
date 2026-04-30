using System;
using Components;
using Configs.Impl;
using Data.HelperClass;
using KoboldUi.Element.Controller;
using Services;
using UI.Views;
using UniRx;
using UnityEngine;

namespace UI.Controllers
{
    public class GameSpeedometerController : AUiController<GameSpeedometerView>
    {
        private readonly IEventService _eventService;
        private readonly CarUISmoothingParameters _carUISmoothingParameters;
        private readonly CarSelectionParameters _carSelectionParameters;
        
        private float _uiSpeed;
        private float _uiBackSpeed;
        private float _uiGear;
        private float _uiRpm;
        private float _maxSpeed;
        private float _maxRpm;

        public GameSpeedometerController(
            IEventService eventService,
            CarUISmoothingParameters carUISmoothingParameters,
            CarSelectionParameters carSelectionParameters)
        {
            _eventService = eventService;
            _carUISmoothingParameters = carUISmoothingParameters;
            _carSelectionParameters = carSelectionParameters;
        }

        public override void Initialize()
        {
            if (_eventService == null)
                return;

            _eventService.CarSetupChangedStream.Subscribe(OnCarSetupAspectChanged).AddTo(View);
            _eventService.CarSelectionChangedStream.Subscribe(_ => OnCarSelectionChanged()).AddTo(View);
            CacheCarLimits();
        }

        private void CacheCarLimits()
        {
            if (_carSelectionParameters == null)
                return;

            if (_carSelectionParameters != null)
                _maxSpeed = _carSelectionParameters.GetSpeedMaxKmh();

            var movementParameters = _carSelectionParameters.MovementParameters;
            if (movementParameters != null && movementParameters.Vertical != null)
                _maxRpm = movementParameters.Vertical.MaxRpm;
        }

        private void OnCarSelectionChanged()
        {
            CacheCarLimits();
            _uiSpeed = 0f;
            _uiBackSpeed = 0f;
            _uiGear = 0f;
            _uiRpm = 0f;
            UpdateSpeedometer();
        }

        private void OnCarSetupAspectChanged(CarSetupAspect aspect)
        {
            var targetSpeed = aspect.Speed.Value;
            var targetBackSpeed = aspect.BackSpeed.Value;
            float targetGear = aspect.Gear.Value;
            var targetRpm = aspect.EngineRpm.Value;

            var absForward = Mathf.Abs(targetSpeed);
            var absBackward = Mathf.Abs(targetBackSpeed);
            var maxAbsSpeed = Mathf.Max(absForward, absBackward);

            var smoothingSettings = _carUISmoothingParameters.SmoothingSetup;
            var speedSmoothing = GetSpeedSmoothing(maxAbsSpeed, smoothingSettings);

            _uiSpeed = Smooth(_uiSpeed, targetSpeed, speedSmoothing);
            _uiBackSpeed = Smooth(_uiBackSpeed, targetBackSpeed, speedSmoothing);
            _uiGear = Smooth(_uiGear, targetGear, speedSmoothing);
            _uiRpm = Smooth(_uiRpm, targetRpm, smoothingSettings.RpmSmoothing);

            UpdateSpeedometer();
        }

        private void UpdateSpeedometer()
        {
            if (View.SpeedText != null)
                View.SpeedText.text = _uiSpeed > _uiBackSpeed ? $"{_uiSpeed:0}" : $"{_uiBackSpeed:0}";

            if (View.GearText != null)
            {
                var gear = (float)Math.Round(_uiGear);
                View.GearText.text = gear switch
                {
                    < 0 => "R",
                    > 0 => $"{_uiGear:0}",
                    0 => "N",
                    _ => View.GearText.text
                };
            }

            if (View.RpmFill == null)
                return;

            var rpmMax = _maxRpm > 0f ? _maxRpm : 1f;
            var normalizedRpm = Mathf.Clamp01(_uiRpm / rpmMax);
            var targetFill = Mathf.Clamp01(View.RpmMaxFill) * normalizedRpm;
            View.RpmFill.fillAmount = Mathf.Lerp(View.RpmFill.fillAmount, targetFill, Time.deltaTime);
        }

        private float Smooth(float current, float target, float smoothing)
        {
            if (smoothing <= 0f)
                return target;

            var time = 1f - Mathf.Exp(-smoothing * Time.deltaTime);
            return Mathf.Lerp(current, target, time);
        }

        private float GetSpeedSmoothing(float speedKmh, CarSmoothingSetup setup)
        {
            var baseValue = setup.SpeedSmoothing;
            var normalized = Mathf.InverseLerp(0f, _maxSpeed, speedKmh);
            var smoothingMultiplier = Mathf.Lerp(1f, setup.MaxSmoothing, normalized);

            return baseValue * smoothingMultiplier;
        }
    }
}