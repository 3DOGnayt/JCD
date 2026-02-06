using System;
using Components;
using Configs.Impl;
using Data.Helpers;
using Services;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.Helpers
{
    public class CarParametersView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _parametersText;
        [SerializeField] private TMP_Text _parametersValue;
        [Space]
        [SerializeField] private TMP_Text _speed;
        [SerializeField] private TMP_Text _gear;
        [SerializeField] private Image _rpm;
        [SerializeField] private float _rpmMaxFill = 0.8f;

        private ILoadingService _loadingService;
        private CarUISmoothingParameters _carUISmoothingParameters;
        private GameSelectionParameters _gameSelectionParameters;
        private IDisposable _carSetupChangedDisposable;

        private float _uiSpeed;
        private float _uiBackSpeed;
        private float _uiGear;
        private float _uiRpm;
        private float _uiDrift;
        
        private float _maxSpeed;
        private float _maxRpm;

        [Inject]
        public void Construct(
            ILoadingService loadingService,
            CarUISmoothingParameters carUISmoothingParameters,
            GameSelectionParameters gameSelectionParameters)
        {
            _loadingService = loadingService;
            _carUISmoothingParameters = carUISmoothingParameters;
            _gameSelectionParameters = gameSelectionParameters;
        }

        private void OnEnable()
        {
            if (_loadingService == null)
                return;

            _carSetupChangedDisposable = _loadingService.CarSetupChangedStream.Subscribe(OnCarSetupAspectChanged);
            CacheCarLimits();
        }

        private void OnDisable()
        {
            _carSetupChangedDisposable?.Dispose();
        }

        private void Awake()
        {
            _parametersText.text = "Km/h\n" +
                                   "Back Speed\n" +
                                   "Gearbox\n" +
                                   "Engine Rpm\n" +
                                   "\n" +
                                   "BrakeInput\n" +
                                   "HandbrakeInput\n" +
                                   "Drift Multiplier";
        }

        private void OnCarSetupAspectChanged(CarSetupAspect aspect)
        {
            var targetSpeed = aspect.Speed.Value;
            var targetBackSpeed = aspect.BackSpeed.Value;
            float targetGear = aspect.Gear.Value;
            var targetRpm = aspect.EngineRpm.Value;
            var targetDrift = aspect.DriftMultiplier.Value;

            var absForward = Mathf.Abs(targetSpeed);
            var absBackward = Mathf.Abs(targetBackSpeed);
            var maxAbsSpeed = Mathf.Max(absForward, absBackward);

            var smoothingSettings = _carUISmoothingParameters.SmoothingSetup;
            var speedSmoothing = GetSpeedSmoothing(maxAbsSpeed, smoothingSettings);

            _uiSpeed = Smooth(_uiSpeed, targetSpeed, speedSmoothing);
            _uiBackSpeed = Smooth(_uiBackSpeed, targetBackSpeed, speedSmoothing);
            _uiGear = Smooth(_uiGear, targetGear, speedSmoothing);
            _uiRpm = Smooth(_uiRpm, targetRpm, smoothingSettings.RpmSmoothing);
            _uiDrift = Smooth(_uiDrift, targetDrift, smoothingSettings.DriftSmoothing);

            //todo: remove after all
            _parametersValue.text =
                $"{_uiSpeed:0} :\n{_uiBackSpeed:0} :\n{_uiGear:0} :\n{_uiRpm:0} :\n" +
                $"\n{aspect.BrakeInput.Value} :\n{aspect.HandbrakeInput.Value} :\n{_uiDrift:0.00} :";

            UpdateSpeedometer();
        }

        private void CacheCarLimits()
        {
            if (_gameSelectionParameters == null)
                return;

            var selectedCar = _gameSelectionParameters.SelectedCar;
            if (selectedCar == null)
                return;

            var carSetup = selectedCar.CarSetup;
            if (carSetup == null)
                return;

            _maxSpeed = carSetup.SpeedMax;
            _maxRpm = carSetup.EngineRpmMax;
        }

        private void UpdateSpeedometer()
        {
            _speed.text = _uiSpeed > _uiBackSpeed ? $"{_uiSpeed:0}" : $"{_uiBackSpeed:0}";

            var gear = (float)Math.Round(_uiGear);

            _gear.text = gear switch
            {
                < 0 => "R",
                > 0 => $"{_uiGear:0}",
                0 => "N",
                _ => _gear.text
            };

            var rpmMax = _maxRpm > 0f ? _maxRpm : 1f;
            var normalizedRpm = Mathf.Clamp01(_uiRpm / rpmMax);
            var targetFill = Mathf.Clamp01(_rpmMaxFill) * normalizedRpm;
            _rpm.fillAmount = Mathf.Lerp(_rpm.fillAmount, targetFill, Time.deltaTime);
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
