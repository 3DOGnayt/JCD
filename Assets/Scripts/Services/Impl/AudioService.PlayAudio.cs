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

        [Inject]
        public void Construct(AudioCatalogParameters audioCatalogParameters)
        {
            _audioCatalogParameters = audioCatalogParameters;
        }
        
        private float _uiVolume;
        private float _uiPitch;
        private float _musicVolume;
        private bool _musicLoop;
        private float _sfx2DVolume;
        private float _sfx2DPitch;
        private float _sfx3DVolume;
        private float _sfx3DPitch;
        private Vector3 _sfx3DPosition;

        public void PlayMusicAudio(EAudioType type, EAudioSubType subType, float volume = 1f, bool loop = true)
        {
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

        public void PlaySfx2DAudio(EAudioType type, EAudioSubType subType, float volume = 1f, float pitch = 1f)
        {
            _sfx2DVolume = volume;
            _sfx2DPitch = pitch;
            PlayAudio(type, subType, PlaySfx2DClip);
        }

        public void PlaySfx3DAudio(EAudioType type, EAudioSubType subType, Vector3 position, float volume = 1f, float pitch = 1f)
        {
            _sfx3DPosition = position;
            _sfx3DVolume = volume;
            _sfx3DPitch = pitch;
            PlayAudio(type, subType, PlaySfx3DClip);
        }

        private void PlayMusicClip(AudioClip clip) => PlayMusic(clip, _musicVolume, _musicLoop);
        private void PlayUiClip(AudioClip clip) => PlayUi(clip, _uiVolume, _uiPitch);
        private void PlaySfx2DClip(AudioClip clip) => PlaySfx2D(clip, _sfx2DVolume, _sfx2DPitch);
        private void PlaySfx3DClip(AudioClip clip) => PlaySfx3D(clip, _sfx3DPosition, _sfx3DVolume, _sfx3DPitch);

        private void PlayAudio(EAudioType audioType, EAudioSubType audioSubType, Action<AudioClip> play)
        {
            var setups = _audioCatalogParameters.AudioSetups;

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
                    
                    play(settingsEntry.AudioClip);
                    return;
                }
            }

            // for (var i = 0; i < setups.Count; i++) // ne po tomy
            // {
            //     var setup = setups[i];
            //
            //     if (setup.AudioType == audioType && setup.AudioSettingsEntry[i].AudioSubType == audioSubType) // nepravilno
            //     {
            //         play(setup.AudioSettingsEntry[i].AudioClip);
            //         return;
            //     }
            // }
        }
    }
}
