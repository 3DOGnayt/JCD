using Components;
using Configs.Impl;
using Data.Enums;
using Data.Struct;
using Helpers.Audio;
using Scellecs.Morpeh;
using Services;
using UnityEngine;
using Zenject;

namespace Systems.Car
{
    public class SkidAudioSystem : IFixedSystem
    {
        [Inject] public World World { get; set; }
        [Inject] private AudioCatalogParameters _audioCatalogParameters;
        [Inject] private AudioSelectionParameters _audioSelectionParameters;
        [Inject] private CarSkidmarksParameters _skidmarksParameters;
        [Inject] private IAudioService _audioService;

        private Filter _cars;
        private Stash<SkidmarksComponent> _skidmarksStash;
        private Stash<SkidAudioComponent> _skidAudioStash;
        private Stash<SpeedComponent> _speedStash;
        private Stash<BackSpeedComponent> _backSpeedStash;
        private Stash<SpeedMaxComponent> _speedMaxStash;

        private AudioClip _skidClip;
        private float _catalogVolume = 1f;
        private float _fadeIn = 8f;
        private float _fadeOut = 6f;
        private float _userSfx = 1f;

        public void OnAwake()
        {
            _cars = World.Filter
                .With<SkidmarksComponent>()
                .With<SkidAudioComponent>()
                .With<SpeedComponent>()
                .With<BackSpeedComponent>()
                .With<SpeedMaxComponent>()
                .Build();
            
            _skidmarksStash = World.GetStash<SkidmarksComponent>();
            _skidAudioStash = World.GetStash<SkidAudioComponent>();
            _speedStash = World.GetStash<SpeedComponent>();
            _backSpeedStash = World.GetStash<BackSpeedComponent>();
            _speedMaxStash = World.GetStash<SpeedMaxComponent>();

            LoadSkidClip();
        }

        public void OnUpdate(float deltaTime)
        {
            if (_skidClip == null)
                return;

            UpdateSettingsCacheIfNeeded();

            foreach (var car in _cars)
                UpdateCarAudio(car, deltaTime);
        }

        public void Dispose() { }

        private float GetUserSfxVolume()
        {
            if (_audioSelectionParameters == null || _audioSelectionParameters.VolumeSetup == null)
                return 1f;

            return _audioSelectionParameters.VolumeSetup.Sfx;
        }

        private void UpdateCarAudio(Entity car, float deltaTime)
        {
            var skidNow = _skidmarksStash.Get(car).Value;
            ref var audioComp = ref _skidAudioStash.Get(car);

            UpdateVolume(ref audioComp, skidNow, deltaTime);
            var speed01 = GetSpeed01(car);
            var pitch = GetSkidPitch(car, speed01);

            if (audioComp.CurrentVolume > 0f)
            {
                EnsureLoopSource(ref audioComp, pitch);
                ApplySourceVolume(audioComp, pitch);
            }
            else if (audioComp.Source != null)
            {
                StopLoopSource(ref audioComp);
            }
        }

        private void UpdateVolume(ref SkidAudioComponent audioComp, bool skidNow, float deltaTime)
        {
            var target = skidNow ? _catalogVolume * _userSfx : 0f;
            var speed = skidNow ? _fadeIn : _fadeOut;
            audioComp.CurrentVolume = Mathf.MoveTowards(audioComp.CurrentVolume, target, speed * deltaTime);
        }

        private void EnsureLoopSource(ref SkidAudioComponent audioComp, float pitch)
        {
            if (audioComp.Source == null && _audioService != null)
                audioComp.Source = _audioService.PlaySfx2DAudio(EAudioType.Sfx, EAudioSubType.Sfx_Tire, 1f, pitch, true);
        }

        private static void ApplySourceVolume(SkidAudioComponent audioComp, float pitch)
        {
            var source = audioComp.Source;
            if (source == null)
                return;

            if (!source.isPlaying)
                source.Play();

            source.volume = audioComp.CurrentVolume;
            source.pitch = pitch;
        }

        private static void StopLoopSource(ref SkidAudioComponent audioComp)
        {
            var pooled = audioComp.Source.GetComponent<PooledAudioSource>();
            if (pooled != null)
                pooled.StopAndRelease();
            else if (audioComp.Source.isPlaying)
                audioComp.Source.Stop();

            audioComp.Source = null;
        }

        private void UpdateSettingsCacheIfNeeded()
        {
            var fadeIn = _skidmarksParameters != null ? _skidmarksParameters.FadeInSpeed : 8f;
            var fadeOut = _skidmarksParameters != null ? _skidmarksParameters.FadeOutSpeed : 6f;
            var userSfx = GetUserSfxVolume();

            if (!Mathf.Approximately(_fadeIn, fadeIn))
                _fadeIn = fadeIn;

            if (!Mathf.Approximately(_fadeOut, fadeOut))
                _fadeOut = fadeOut;

            if (!Mathf.Approximately(_userSfx, userSfx))
                _userSfx = userSfx;
        }

        private float GetSpeed01(Entity car)
        {
            var forwardSpeed = Mathf.Max(0f, _speedStash.Get(car).Value);
            var backwardSpeed = Mathf.Max(0f, _backSpeedStash.Get(car).Value);
            var speed = Mathf.Max(forwardSpeed, backwardSpeed);
            var maxSpeed = _speedMaxStash.Get(car).Value;
            if (maxSpeed <= 0f)
                return 0f;

            return Mathf.Clamp01(speed / maxSpeed);
        }

        private static float GetSkidPitch(Entity car, float speed01)
        {
            var basePitch = Mathf.Lerp(1f, 1.5f, speed01);
            var noise = (Mathf.PerlinNoise(Time.time * 0.5f, car.GetHashCode() * 0.01f) - 0.5f) * 0.04f;
            return Mathf.Clamp(basePitch + noise, 1f, 1.5f);
        }

        private void LoadSkidClip()
        {
            if (_audioCatalogParameters == null)
                return;

            var setups = _audioCatalogParameters.AudioSetups;
            if (setups == null)
                return;

            for (var i = 0; i < setups.Count; i++)
            {
                var setup = setups[i];
                if (setup.AudioType != EAudioType.Sfx)
                    continue;

                if (TryGetSkidEntry(setup, out var entry))
                {
                    _skidClip = entry.AudioClip;
                    _catalogVolume = Mathf.Max(0f, entry.Volume);
                    return;
                }
            }
        }

        private bool TryGetSkidEntry(AudioSettingsSetup setup, out AudioSettingsEntry entry)
        {
            entry = default;

            if (setup.AudioSettingsEntry == null)
                return false;

            for (var i = 0; i < setup.AudioSettingsEntry.Count; i++)
            {
                var candidate = setup.AudioSettingsEntry[i];
                if (candidate.AudioSubType != EAudioSubType.Sfx_Tire)
                    continue;

                entry = candidate;
                return true;
            }

            return false;
        }
    }
}