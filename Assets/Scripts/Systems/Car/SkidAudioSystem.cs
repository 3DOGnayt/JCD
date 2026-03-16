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

        private AudioClip _skidClip;
        private float _catalogVolume = 1f;

        public void OnAwake()
        {
            _cars = World.Filter
                .With<SkidmarksComponent>()
                .With<SkidAudioComponent>()
                .Build();
            
            _skidmarksStash = World.GetStash<SkidmarksComponent>();
            _skidAudioStash = World.GetStash<SkidAudioComponent>();

            LoadSkidClip();
        }

        public void OnUpdate(float deltaTime)
        {
            if (_skidClip == null)
                return;

            var fadeIn = _skidmarksParameters != null ? _skidmarksParameters.FadeInSpeed : 8f;
            var fadeOut = _skidmarksParameters != null ? _skidmarksParameters.FadeOutSpeed : 6f;
            var userSfx = GetUserSfxVolume();

            foreach (var car in _cars)
                UpdateCarAudio(car, fadeIn, fadeOut, userSfx, deltaTime);
        }

        public void Dispose() { }

        private float GetUserSfxVolume()
        {
            if (_audioSelectionParameters == null || _audioSelectionParameters.VolumeSetup == null)
                return 1f;

            return _audioSelectionParameters.VolumeSetup.Sfx;
        }

        private void UpdateCarAudio(Entity car, float fadeIn, float fadeOut, float userSfx, float deltaTime)
        {
            var skidNow = _skidmarksStash.Get(car).Value;
            ref var audioComp = ref _skidAudioStash.Get(car);

            UpdateVolume(ref audioComp, skidNow, fadeIn, fadeOut, userSfx, deltaTime);

            if (audioComp.CurrentVolume > 0f)
            {
                EnsureLoopSource(ref audioComp);
                ApplySourceVolume(audioComp);
            }
            else if (audioComp.Source != null)
            {
                StopLoopSource(ref audioComp);
            }
        }

        private void UpdateVolume(ref SkidAudioComponent audioComp, bool skidNow, float fadeIn, float fadeOut, float userSfx, float deltaTime)
        {
            var target = skidNow ? _catalogVolume * userSfx : 0f;
            var speed = skidNow ? fadeIn : fadeOut;
            audioComp.CurrentVolume = Mathf.MoveTowards(audioComp.CurrentVolume, target, speed * deltaTime);
        }

        private void EnsureLoopSource(ref SkidAudioComponent audioComp)
        {
            if (audioComp.Source == null && _audioService != null)
                audioComp.Source = _audioService.PlaySfx2DAudio(EAudioType.Sfx, EAudioSubType.Sfx_Tire, 1f, 1f, true);
        }

        private static void ApplySourceVolume(SkidAudioComponent audioComp)
        {
            var source = audioComp.Source;
            if (source == null)
                return;

            if (!source.isPlaying)
                source.Play();

            source.volume = audioComp.CurrentVolume;
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