using Data;
using Services.Impl;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class ServicesInstaller : MonoInstaller
    {
        [SerializeField] private SkidmarksParameters _skidmarksParameters;
        [SerializeField] private SkidSmokeParameters _skidSmokeParameters;
        
        public override void InstallBindings()
        {
            Services();
        }

        private void Services()
        {
            Container.BindInterfacesAndSelfTo<InputService>().AsSingle();
            Container.BindInterfacesAndSelfTo<SkidmarksService>().AsSingle().WithArguments(_skidmarksParameters);
            Container.BindInterfacesAndSelfTo<SkidSmokeService>().AsSingle().WithArguments(_skidSmokeParameters);
        }
    }
}