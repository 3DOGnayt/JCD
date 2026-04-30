using Core;
using Scellecs.Morpeh;
using Services;
using Services.Impl;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class CoreInstaller : MonoInstaller
    {
        [SerializeField] private Startup startup;
        
        public override void InstallBindings()
        {
            Main();
        }

        private void Main()
        {
            var world = World.Create();
            Container.Bind<World>().FromInstance(world).AsSingle();
            
            Container.BindInterfacesTo<WorldLifetimeService>()
                .FromInstance(new WorldLifetimeService(world))
                .AsSingle();
            
            Container.Bind<ISystemService>().To<SystemService>().AsSingle();
            
            Container.Bind<Startup>().FromInstance(startup).AsSingle();
        }
    }
}