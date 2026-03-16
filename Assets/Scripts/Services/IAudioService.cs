using Data.Enums;
using UnityEngine;

namespace Services
{
    public interface IAudioService
    {
        void StopMusic();
        void StopUi();
        void StopAllSfx();
        void StopAllAudio();

        AudioSource PlayMusic(AudioClip clip, float volume = 1f, bool loop = true);
        void PlayUi(AudioClip clip, float volume = 1f, float pitch = 1f);
        AudioSource PlaySfx2D(AudioClip clip, float volume = 1f, float pitch = 1f, bool loop = false);
        void PlaySfx3D(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f);

        void PlayMusicAudio(EAudioType type, EAudioSubType subType, float volume = 1f, bool loop = true);
        void PlayUiAudio(EAudioType type, EAudioSubType subType, float volume = 1f, float pitch = 1f);
        AudioSource PlaySfx2DAudio(EAudioType type, EAudioSubType subType, float volume = 1f, float pitch = 1f, bool loop = false);
        void PlaySfx3DAudio(EAudioType type, EAudioSubType subType, Vector3 position, float volume = 1f, float pitch = 1f);

        void SetAudioVolume(EAudioType type, float value);
        void TransitionSnapshot(string snapshotName, float time);
    }
}