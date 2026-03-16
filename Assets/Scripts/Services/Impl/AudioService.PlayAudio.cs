using System;
using Configs.Impl;
using Data.Enums;
using UnityEngine;
using Zenject;

namespace Services.Impl
{
    public partial class AudioService
    {
        private AudioCatalogParameters _audioCatalogParameters;
        private AudioSelectionParameters _audioSelectionParameters;

        [Inject]
        public void Construct(
            AudioCatalogParameters audioCatalogParameters,
            AudioSelectionParameters audioSelectionParameters
        )
        {
            _audioCatalogParameters = audioCatalogParameters;
            _audioSelectionParameters = audioSelectionParameters;
        }
        
        private float _uiVolume;
        private float _uiPitch;
        private float _musicVolume;
        private bool _musicLoop;
        private float _musicBaseVolume;
        private float _musicCatalogVolume = 1f;
        private float _sfx2DVolume;
        private float _sfx2DPitch;
        private bool _sfx2DLoop;
        private float _sfx3DVolume;
        private float _sfx3DPitch;
        private Vector3 _sfx3DPosition;

        public void PlayMusicAudio(EAudioType type, EAudioSubType subType, float volume = 1f, bool loop = true)
        {
            _musicBaseVolume = volume;
            _musicVolume = volume;
            _musicLoop = loop;
            PlayAudio(type, subType, PlayMusicClip);
        }

        public void PlayUiAudio(EAudioType type, EAudioSubType subType, float volume = 1f, float pitch = 1f)
        {
            _uiVolume = volume;
            _uiPitch = pitch;
            PlayAudio(type, subType, PlayUiClip);
        }

        public AudioSource PlaySfx2DAudio(EAudioType type, EAudioSubType subType, float volume = 1f, float pitch = 1f, bool loop = false)
        {
            _sfx2DVolume = volume;
            _sfx2DPitch = pitch;
            _sfx2DLoop = loop;
            return PlayAudio(type, subType, PlaySfx2DClip);
        }

        public void PlaySfx3DAudio(EAudioType type, EAudioSubType subType, Vector3 position, float volume = 1f, float pitch = 1f)
        {
            _sfx3DPosition = position;
            _sfx3DVolume = volume;
            _sfx3DPitch = pitch;
            PlayAudio(type, subType, PlaySfx3DClip);
        }

        private AudioSource PlayMusicClip(AudioClip clip) => PlayMusic(clip, _musicVolume, _musicLoop);
        private AudioSource PlayUiClip(AudioClip clip)
        {
            PlayUi(clip, _uiVolume, _uiPitch);
            return null;
        }
        private AudioSource PlaySfx2DClip(AudioClip clip) => PlaySfx2D(clip, _sfx2DVolume, _sfx2DPitch, _sfx2DLoop);
        private AudioSource PlaySfx3DClip(AudioClip clip)
        {
            PlaySfx3D(clip, _sfx3DPosition, _sfx3DVolume, _sfx3DPitch);
            return null;
        }

        private AudioSource PlayAudio(EAudioType audioType, EAudioSubType audioSubType, Func<AudioClip, AudioSource> play)
        {
            if (_audioCatalogParameters == null)
                return null;

            var setups = _audioCatalogParameters.AudioSetups;
            if (setups == null)
                return null;

            for (var i = 0; i < setups.Count; i++)
            {
                var setup = setups[i];
                if (setup.AudioType != audioType)
                    continue;

                for (var k = 0; k < setup.AudioSettingsEntry.Count; k++)
                {
                    var settingsEntry = setup.AudioSettingsEntry[k];
                    if (settingsEntry.AudioSubType != audioSubType) 
                        continue;

                    ApplyVolumeFromCatalog(audioType, settingsEntry.Volume);
                    return play(settingsEntry.AudioClip);
                }
            }

            return null;
        }

        private void ApplyVolumeFromCatalog(EAudioType audioType, float volume)
        {
            switch (audioType)
            {
                case EAudioType.Music:
                    _musicCatalogVolume = volume;
                    _musicVolume = _musicBaseVolume * _musicCatalogVolume * GetUserVolume(EAudioType.Music);
                    UpdateMusicVolume();
                    break;
                case EAudioType.Ui:
                    _uiVolume *= volume * GetUserVolume(EAudioType.Ui);
                    break;
                case EAudioType.Sfx:
                    var sfxMultiplier = volume * GetUserVolume(EAudioType.Sfx);
                    _sfx2DVolume *= sfxMultiplier;
                    _sfx3DVolume *= sfxMultiplier;
                    break;
            }
        }

        private float GetUserVolume(EAudioType audioType)
        {
            if (_audioSelectionParameters == null || _audioSelectionParameters.VolumeSetup == null)
                return 1f;

            var settings = _audioSelectionParameters.VolumeSetup;
            return audioType switch
            {
                EAudioType.Music => settings.Music,
                EAudioType.Sfx => settings.Sfx,
                EAudioType.Ui => settings.Ui,
                _ => 1f
            };
        }

        private void UpdateMusicVolume()
        {
            if (_musicSource == null || !_musicSource.isPlaying)
                return;

            _musicVolume = _musicBaseVolume * _musicCatalogVolume * GetUserVolume(EAudioType.Music);
            _musicSource.volume = _musicVolume;
        }
    }
}