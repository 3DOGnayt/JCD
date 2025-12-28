using Configs.Impl;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class CarConfigInstaller : MonoInstaller
    {
        [SerializeField] private CarPresetParameters _carPresetParameters;
        [SerializeField] private CarParameters _carParameters;
        [SerializeField] private CarUISmoothingParameters _carUISmoothingParameters;
        [SerializeField] private CarSkidSmokeParameters _carCarSkidSmokeParameters;
        [SerializeField] private CarLightsParameters _carLightsParameters;
        [SerializeField] private CarCollisionSlideParameters _carCarCollisionSlideParameters;
        
        public override void InstallBindings()
        {
            Container.Bind<CarPresetParameters>().FromInstance(_carPresetParameters).AsSingle();
            Container.Bind<CarParameters>().FromInstance(_carParameters).AsSingle();
            Container.Bind<CarUISmoothingParameters>().FromInstance(_carUISmoothingParameters).AsSingle();
            Container.Bind<CarSkidSmokeParameters>().FromInstance(_carCarSkidSmokeParameters).AsSingle();
            Container.Bind<CarLightsParameters>().FromInstance(_carLightsParameters).AsSingle();
            Container.Bind<CarCollisionSlideParameters>().FromInstance(_carCarCollisionSlideParameters).AsSingle();
        }
    }
}