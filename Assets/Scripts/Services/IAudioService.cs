using Data.Enums;
using UnityEngine;

namespace Services
{
    public interface IAudioService
    {
        AudioSource PlayMusic(AudioClip clip, float volume = 1f, bool loop = true);
        void StopMusic();

        void PlayUi(AudioClip clip, float volume = 1f, float pitch = 1f);
        void PlaySfx2D(AudioClip clip, float volume = 1f, float pitch = 1f);
        void PlaySfx3D(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f);

        void SetAudioVolume(EAudioType type, float value);
        void TransitionSnapshot(string snapshotName, float time);
    }
}
