using Configs.Impl;
using Services.Impl;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class ServicesInstaller : MonoInstaller
    {
        [SerializeField] private CarSkidmarksParameters _carSkidmarksParameters;
        [SerializeField] private CarSkidSmokeParameters _carSkidSmokeParameters;
        [SerializeField] private CarCrashEffectsParameters _carCrashEffectsParameters;
        
        public override void InstallBindings()
        {
            Services();
        }

        private void Services()
        {
            Container.BindInterfacesAndSelfTo<LoadingService>().AsSingle();
            Container.BindInterfacesAndSelfTo<InputService>().AsSingle();
            Container.BindInterfacesAndSelfTo<SkidmarksService>().AsSingle().WithArguments(_carSkidmarksParameters);
            Container.BindInterfacesAndSelfTo<SkidSmokeService>().AsSingle().WithArguments(_carSkidSmokeParameters);
            Container.BindInterfacesAndSelfTo<CrashEffectService>().AsSingle().WithArguments(_carCrashEffectsParameters);
        }
    }
}