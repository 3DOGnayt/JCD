using Components;
using Configs.Impl;
using Data.Enums;
using Data.Struct;
using Scellecs.Morpeh;
using Services;
using UnityEngine;
using Zenject;

namespace Systems.Car
{
    public sealed class EngineAudioSystem : IFixedSystem
    {
        [Inject] public World World { get; set; }
        [Inject] private AudioCatalogParameters _audioCatalogParameters;
        [Inject] private AudioSelectionParameters _audioSelectionParameters;
        [Inject] private CarEngineAudioParameters _carEngineAudioParameters;
        [Inject] private IAudioService _audioService;

        private Filter _cars;
        private Stash<EngineAudioComponent> _engineAudioStash;
        private Stash<SpeedComponent> _speedStash;
        private Stash<SpeedMaxComponent> _speedMaxStash;

        private bool _hasLowOn;
        private bool _hasMedOn;
        private bool _hasHighOn;
        
        private float _lowOnCatalogVolume = 1f;
        private float _medOnCatalogVolume = 1f;
        private float _highOnCatalogVolume = 1f;
        
        private float _userSfx = 1f;
        
        private float _lowPitchMin;
        private float _lowPitchMax;
        private float _medPitchMin;
        private float _medPitchMax;
        private float _highPitchMin;
        private float _highPitchMax;
        
        private float _fadeSpeed;
        private float _lowVolumeScale;
        private float _bandLowMax;
        private float _bandMedMax;
        private float _defaultMaxSpeedKmh;
        private bool _hasParameters;

        public void OnAwake()
        {
            _cars = World.Filter
                .With<EngineAudioComponent>()
                .With<SpeedComponent>()
                .With<SpeedMaxComponent>()
                .Build();

            _engineAudioStash = World.GetStash<EngineAudioComponent>();
            _speedStash = World.GetStash<SpeedComponent>();
            _speedMaxStash = World.GetStash<SpeedMaxComponent>();

            ApplyParameters();
            LoadMotorEntries();
        }

        public void OnUpdate(float deltaTime)
        {
            if (!_hasLowOn && !_hasMedOn && !_hasHighOn)
                return;

            if (!_hasParameters)
                return;

            _userSfx = GetUserSfxVolume();

            foreach (var car in _cars)
                UpdateCarAudio(car, deltaTime);
        }

        public void Dispose() { }

        private void UpdateCarAudio(Entity car, float deltaTime)
        {
            ref var audioComp = ref _engineAudioStash.Get(car);

            var speed01 = GetSpeed01(car);
            var lowPitch = Mathf.Lerp(_lowPitchMin, _lowPitchMax, speed01);
            var medPitch = Mathf.Lerp(_medPitchMin, _medPitchMax, speed01);
            var highPitch = Mathf.Lerp(_highPitchMin, _highPitchMax, speed01);
            var band = GetBand(speed01);

            EnsureLoopSources(ref audioComp, medPitch);

            UpdateLoopVolumes(ref audioComp, band, deltaTime);
            ApplyLoopSettings(audioComp, lowPitch, medPitch, highPitch);
        }

        private void EnsureLoopSources(ref EngineAudioComponent audioComp, float pitch)
        {
            if (_audioService == null)
                return;

            if (_hasLowOn && audioComp.LowSource == null)
                audioComp.LowSource = _audioService.PlaySfx2DAudio(EAudioType.Sfx, EAudioSubType.Sfx_Motor_2, 1f, pitch, true);

            if (_hasMedOn && audioComp.MedSource == null)
                audioComp.MedSource = _audioService.PlaySfx2DAudio(EAudioType.Sfx, EAudioSubType.Sfx_Motor_3, 1f, pitch, true);

            if (_hasHighOn && audioComp.HighSource == null)
                audioComp.HighSource = _audioService.PlaySfx2DAudio(EAudioType.Sfx, EAudioSubType.Sfx_Motor_4, 1f, pitch, true);
        }

        private void UpdateLoopVolumes(ref EngineAudioComponent audioComp, EngineBand band, float deltaTime)
        {
            var lowTarget = band == EngineBand.Low ? _lowOnCatalogVolume * _userSfx * _lowVolumeScale : 0f;
            var medTarget = band == EngineBand.Med ? _medOnCatalogVolume * _userSfx : 0f;
            var highTarget = band == EngineBand.High ? _highOnCatalogVolume * _userSfx : 0f;

            audioComp.LowVolume = Mathf.MoveTowards(audioComp.LowVolume, lowTarget, _fadeSpeed * deltaTime);
            audioComp.MedVolume = Mathf.MoveTowards(audioComp.MedVolume, medTarget, _fadeSpeed * deltaTime);
            audioComp.HighVolume = Mathf.MoveTowards(audioComp.HighVolume, highTarget, _fadeSpeed * deltaTime);
        }

        private static void ApplyLoopSettings(EngineAudioComponent audioComp, float lowPitch, float medPitch, float highPitch)
        {
            ApplySource(audioComp.LowSource, audioComp.LowVolume, lowPitch);
            ApplySource(audioComp.MedSource, audioComp.MedVolume, medPitch);
            ApplySource(audioComp.HighSource, audioComp.HighVolume, highPitch);
        }

        private static void ApplySource(AudioSource source, float volume, float pitch)
        {
            if (source == null)
                return;

            if (!source.isPlaying)
                source.Play();

            source.volume = volume;
            source.pitch = pitch;
        }

        private float GetSpeed01(Entity car)
        {
            var maxSpeed = _speedMaxStash.Get(car).Value;
            if (maxSpeed <= 0f)
                maxSpeed = _defaultMaxSpeedKmh;

            var speed = Mathf.Max(0f, _speedStash.Get(car).Value);
            return Mathf.Clamp01(speed / maxSpeed);
        }

        private float GetUserSfxVolume()
        {
            if (_audioSelectionParameters == null || _audioSelectionParameters.VolumeSetup == null)
                return 1f;

            return _audioSelectionParameters.VolumeSetup.Sfx;
        }

        private void ApplyParameters()
        {
            if (_carEngineAudioParameters == null)
            {
                Debug.LogError($"{nameof(EngineAudioSystem)}: CarEngineAudioParameters is null.");
                _hasParameters = false;
                return;
            }

            _lowPitchMin = _carEngineAudioParameters.LowPitchMin;
            _lowPitchMax = _carEngineAudioParameters.LowPitchMax;
            _medPitchMin = _carEngineAudioParameters.MedPitchMin;
            _medPitchMax = _carEngineAudioParameters.MedPitchMax;
            _highPitchMin = _carEngineAudioParameters.HighPitchMin;
            _highPitchMax = _carEngineAudioParameters.HighPitchMax;
            _fadeSpeed = _carEngineAudioParameters.FadeSpeed;
            _lowVolumeScale = _carEngineAudioParameters.LowVolumeScale;
            _bandLowMax = _carEngineAudioParameters.BandLowMax;
            _bandMedMax = _carEngineAudioParameters.BandMedMax;
            _defaultMaxSpeedKmh = _carEngineAudioParameters.DefaultMaxSpeedKmh;
            _hasParameters = true;
        }

        private void LoadMotorEntries()
        {
            _hasLowOn = TryGetEntry(EAudioSubType.Sfx_Motor_2, out var lowOnEntry);
            if (_hasLowOn)
                _lowOnCatalogVolume = Mathf.Max(0f, lowOnEntry.Volume);

            _hasMedOn = TryGetEntry(EAudioSubType.Sfx_Motor_3, out var medOnEntry);
            if (_hasMedOn)
                _medOnCatalogVolume = Mathf.Max(0f, medOnEntry.Volume);

            _hasHighOn = TryGetEntry(EAudioSubType.Sfx_Motor_4, out var highOnEntry);
            if (_hasHighOn)
                _highOnCatalogVolume = Mathf.Max(0f, highOnEntry.Volume);
        }

        private bool TryGetEntry(EAudioSubType subType, out AudioSettingsEntry entry)
        {
            entry = default;

            if (_audioCatalogParameters == null)
                return false;

            var setups = _audioCatalogParameters.AudioSetups;
            if (setups == null)
                return false;

            for (var i = 0; i < setups.Count; i++)
            {
                var setup = setups[i];
                if (setup.AudioType != EAudioType.Sfx || setup.AudioSettingsEntry == null)
                    continue;

                for (var k = 0; k < setup.AudioSettingsEntry.Count; k++)
                {
                    var candidate = setup.AudioSettingsEntry[k];
                    if (candidate.AudioSubType != subType)
                        continue;

                    entry = candidate;
                    return true;
                }
            }

            return false;
        }

        private EngineBand GetBand(float rpm01)
        {
            if (rpm01 <= _bandLowMax)
                return EngineBand.Low;

            if (rpm01 <= _bandMedMax)
                return EngineBand.Med;

            return EngineBand.High;
        }

        private enum EngineBand
        {
            Low = 0,
            Med = 1,
            High = 2
        }
    }
}