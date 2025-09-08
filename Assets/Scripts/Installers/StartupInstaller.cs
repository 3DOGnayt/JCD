using Core;
using Scellecs.Morpeh;
using Services.Impl;
using Systems;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class StartupInstaller : MonoInstaller
    {
        [SerializeField] private Startup startup;
        
        public override void InstallBindings()
        {
            Main();
            Services();
            Systems();
        }

        private void Main()
        {
            Container.Bind<World>().FromMethod(_ => World.Default).AsSingle();
            Container.Bind<Startup>().FromInstance(startup).AsSingle();
        }
        
        private void Systems()
        {
            Container.Bind<ISystem>().To<PlayerSpawnSystem>().AsSingle();
        }
        
        private void Services()
        {
            Container.BindInterfacesAndSelfTo<InputService>().AsSingle();
        }
    }
}