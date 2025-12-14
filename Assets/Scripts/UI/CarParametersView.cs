using Components;
using Configs.Helpers;
using Configs.Impl;
using Signals;
using TMPro;
using UnityEngine;
using Zenject;

namespace UI
{
    public class CarParametersView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _parametersText;
        [SerializeField] private TMP_Text _parametersValue;

        private CarUISmoothing _carUISmoothing;
        private SignalBus _signalBus;

        private float _uiSpeed;
        private float _uiBackSpeed;
        private float _uiRpm;
        private float _uiDrift;
        private float _maxSpeed;

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
        }

        private void OnDisable()
        {
            _signalBus.Unsubscribe<ComponentChangeSignal<CarSetupAspect>>(OnCarSetupAspectChanged);
            _signalBus.Unsubscribe<ComponentChangeSignal<SpeedMaxComponent>>(OnCarSpeedMaxChanged);
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

        private void OnCarSetupAspectChanged(ComponentChangeSignal<CarSetupAspect> signal)
        {
            var aspect = signal.Component;

            var targetSpeed = aspect.Speed.Value;
            var targetBackSpeed = aspect.BackSpeed.Value;
            var targetRpm = aspect.EngineRpm.Value;
            var targetDrift = aspect.DriftMultiplier.Value;

            var absForward = Mathf.Abs(targetSpeed);
            var absBackward = Mathf.Abs(targetBackSpeed);
            var maxAbsSpeed = Mathf.Max(absForward, absBackward);

            var smoothingSettings = _carUISmoothing.SmoothingSettings;
            var speedSmoothing = GetSpeedSmoothing(maxAbsSpeed, smoothingSettings);

            _uiSpeed = Smooth(_uiSpeed, targetSpeed, speedSmoothing);
            _uiBackSpeed = Smooth(_uiBackSpeed, targetBackSpeed, speedSmoothing);
            _uiRpm = Smooth(_uiRpm, targetRpm, smoothingSettings.RpmSmoothing);
            _uiDrift = Smooth(_uiDrift, targetDrift, smoothingSettings.DriftSmoothing);

            _parametersValue.text =
                $"{_uiSpeed:0} :\n{_uiBackSpeed:0} :\n{aspect.Gear.Value:0} :\n{_uiRpm:0} :\n" +
                $"\n{aspect.BrakeInput.Value} :\n{aspect.HandbrakeInput.Value} :\n{_uiDrift:0.00} :";
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
