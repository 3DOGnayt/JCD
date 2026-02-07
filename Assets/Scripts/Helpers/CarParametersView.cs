using System;
using Components;
using Configs.Impl;
using Data.Helpers;
using Services;
using TMPro;
using UniRx;
using UnityEngine;
using Zenject;

namespace Helpers
{
    public class CarParametersView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _parametersText;
        [SerializeField] private TMP_Text _parametersValue;

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
