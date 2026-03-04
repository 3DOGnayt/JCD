using Configs.Impl;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class AudioConfigsInstaller : MonoInstaller
    {
        [SerializeField] private AudioCatalogParameters _audioCatalogParameters;
        [SerializeField] private AudioSelectionParameters _audioSelectionParameters;

        public override void InstallBindings()
        {
            Container.Bind<AudioCatalogParameters>().FromInstance(_audioCatalogParameters).AsSingle();
            Container.Bind<AudioSelectionParameters>().FromInstance(_audioSelectionParameters).AsSingle();
        }
    }
}