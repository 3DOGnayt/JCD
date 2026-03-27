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
        [Inject] private CarSelectionParameters _carSelectionParameters;
        [Inject] private IAudioService _audioService;
        [Inject] private IEventService _eventService;
        [Inject] private IGameSessionService _gameSessionService;

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
        private CarEngineAudioParameters _engineAudioParameters;

        public void OnAwake()
        {
            _cars = World.Filter
                .With<PlayerTagComponent>()
                .With<EngineAudioComponent>()
                .With<SpeedComponent>()
                .With<SpeedMaxComponent>()
                .Build();

            _engineAudioStash = World.GetStash<EngineAudioComponent>();
            _speedStash = World.GetStash<SpeedComponent>();
            _speedMaxStash = World.GetStash<SpeedMaxComponent>();

            LoadMotorEntries();
        }

        public void OnUpdate(float deltaTime)
        {
            if (!_hasLowOn && !_hasMedOn && !_hasHighOn)
                return;

            if (!IsAudioAllowed())
                return;

            _engineAudioParameters = _carSelectionParameters != null
                ? _carSelectionParameters.SelectedCarEngineAudioParameters
                : null;

            if (_engineAudioParameters == null)
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
            var lowPitch = Mathf.Lerp(_engineAudioParameters.LowPitchMin, _engineAudioParameters.LowPitchMax, speed01);
            var medPitch = Mathf.Lerp(_engineAudioParameters.MedPitchMin, _engineAudioParameters.MedPitchMax, speed01);
            var highPitch = Mathf.Lerp(_engineAudioParameters.HighPitchMin, _engineAudioParameters.HighPitchMax, speed01);
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

        private bool IsAudioAllowed()
        {
            if (_gameSessionService != null && _gameSessionService.Target != EGameSessionTarget.Game)
                return false;

            if (_eventService != null && !_eventService.IsLoadingCompleted.Value)
                return false;

            return true;
        }

        private void UpdateLoopVolumes(ref EngineAudioComponent audioComp, EngineBand band, float deltaTime)
        {
            var lowTarget = band == EngineBand.Low
                ? _lowOnCatalogVolume * _userSfx * _engineAudioParameters.LowVolumeScale
                : 0f;
            var medTarget = band == EngineBand.Med ? _medOnCatalogVolume * _userSfx : 0f;
            var highTarget = band == EngineBand.High ? _highOnCatalogVolume * _userSfx : 0f;

            var fadeSpeed = _engineAudioParameters.FadeSpeed;
            audioComp.LowVolume = Mathf.MoveTowards(audioComp.LowVolume, lowTarget, fadeSpeed * deltaTime);
            audioComp.MedVolume = Mathf.MoveTowards(audioComp.MedVolume, medTarget, fadeSpeed * deltaTime);
            audioComp.HighVolume = Mathf.MoveTowards(audioComp.HighVolume, highTarget, fadeSpeed * deltaTime);
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
                maxSpeed = _engineAudioParameters.DefaultMaxSpeedKmh;

            var speed = Mathf.Max(0f, _speedStash.Get(car).Value);
            return Mathf.Clamp01(speed / maxSpeed);
        }

        private float GetUserSfxVolume()
        {
            if (_audioSelectionParameters == null || _audioSelectionParameters.VolumeSetup == null)
                return 1f;

            return _audioSelectionParameters.VolumeSetup.Sfx;
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
            if (rpm01 <= _engineAudioParameters.BandLowMax)
                return EngineBand.Low;

            if (rpm01 <= _engineAudioParameters.BandMedMax)
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