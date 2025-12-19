using System;
using Components;
using Configs.Impl;
using Data.Helpers;
using Signals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    public class CarParametersView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _parametersText;
        [SerializeField] private TMP_Text _parametersValue;
        
        [SerializeField] private TMP_Text _speed;
        [SerializeField] private TMP_Text _gear;
        [SerializeField] private Image _rpm;

        private CarUISmoothing _carUISmoothing;
        private SignalBus _signalBus;

        private float _uiSpeed;
        private float _uiBackSpeed;
        private float _uiGear;
        private float _uiRpm;
        private float _uiDrift;
        
        private float _maxSpeed;
        private float _maxRpm;

        [Inject]
        public void Construct(SignalBus signalBus, CarUISmoothing carUISmoothing)
        {
            _signalBus = signalBus;
            _carUISmoothing = carUISmoothing;
        }

        private void OnEnable()
        {
            _signalBus.Subscribe<ComponentChangeSignal<CarSetupAspect>>(OnCarSetupAspectChanged);
            _signalBus.Subscribe<ComponentChangeSignal<SpeedMaxComponent>>(OnCarSpeedMaxChanged);
            _signalBus.Subscribe<ComponentChangeSignal<EngineRpmMaxComponent>>(OnCarRpmMaxChanged);
        }

        private void OnDisable()
        {
            _signalBus.Unsubscribe<ComponentChangeSignal<CarSetupAspect>>(OnCarSetupAspectChanged);
            _signalBus.Unsubscribe<ComponentChangeSignal<SpeedMaxComponent>>(OnCarSpeedMaxChanged);
            _signalBus.Unsubscribe<ComponentChangeSignal<EngineRpmMaxComponent>>(OnCarRpmMaxChanged);
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

        private void OnCarSpeedMaxChanged(ComponentChangeSignal<SpeedMaxComponent> signal)
        {
            _maxSpeed = signal.Component.Value;
        }

        private void OnCarRpmMaxChanged(ComponentChangeSignal<EngineRpmMaxComponent> signal)
        {
            _maxRpm = signal.Component.Value;
        }

        private void OnCarSetupAspectChanged(ComponentChangeSignal<CarSetupAspect> signal)
        {
            var aspect = signal.Component;

            var targetSpeed = aspect.Speed.Value;
            var targetBackSpeed = aspect.BackSpeed.Value;
            float targetGear = aspect.Gear.Value;
            var targetRpm = aspect.EngineRpm.Value;
            var targetDrift = aspect.DriftMultiplier.Value;

            var absForward = Mathf.Abs(targetSpeed);
            var absBackward = Mathf.Abs(targetBackSpeed);
            var maxAbsSpeed = Mathf.Max(absForward, absBackward);

            var smoothingSettings = _carUISmoothing.SmoothingSettings;
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

            _rpm.fillAmount = Mathf.Lerp(_rpm.fillAmount, _uiRpm / _maxRpm, Time.deltaTime);
        }

        private float Smooth(float current, float target, float smoothing)
        {
            if (smoothing <= 0f)
                return target;

            var time = 1f - Mathf.Exp(-smoothing * Time.deltaTime);
            return Mathf.Lerp(current, target, time);
        }

        private float GetSpeedSmoothing(float speedKmh, CarSmoothingSettings settings)
        {
            var baseValue = settings.SpeedSmoothing;
            var normalized = Mathf.InverseLerp(0f, _maxSpeed, speedKmh);
            var smoothingMultiplier = Mathf.Lerp(1f, settings.MaxSmoothing, normalized);

            return baseValue * smoothingMultiplier;
        }
    }
}