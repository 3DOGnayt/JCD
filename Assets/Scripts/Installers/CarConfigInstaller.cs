using Configs.Impl;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class CarConfigInstaller : MonoInstaller
    {
        [SerializeField] private CarPreset _carPreset;
        [SerializeField] private CarParameters _carParameters;
        [SerializeField] private CarUISmoothing _carUISmoothing;
        
        public override void InstallBindings()
        {
            Container.Bind<CarPreset>().FromInstance(_carPreset).AsSingle();
            Container.Bind<CarParameters>().FromInstance(_carParameters).AsSingle();
            Container.Bind<CarUISmoothing>().FromInstance(_carUISmoothing).AsSingle();
        }
    }
}