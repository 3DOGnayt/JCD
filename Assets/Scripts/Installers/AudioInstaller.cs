using Services.Impl;
using UnityEngine;
using UnityEngine.Audio;
using Zenject;

namespace Installers
{
    public class AudioInstaller : MonoInstaller
    {
        [SerializeField] private AudioMixer _audioMixer;
        [SerializeField] private AudioMixerGroup _musicMixerGroup;
        [SerializeField] private AudioMixerGroup _sfxMixerGroup;
        [SerializeField] private AudioMixerGroup _uiMixerGroup;
        [SerializeField] private GameObject _soundFxPrefab;
        [SerializeField] private int _sfxPoolSize = 16;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<AudioService>().AsSingle().WithArguments(
                _audioMixer,
                _musicMixerGroup,
                _sfxMixerGroup,
                _uiMixerGroup,
                _soundFxPrefab,
                _sfxPoolSize);
        }
    }
}