using Core;
using Scellecs.Morpeh;
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
            Systems();
        }

        private void Main()
        {
            Container.Bind<World>().FromMethod(_ => World.Default).AsSingle();
            Container.Bind<Startup>().FromInstance(startup).AsSingle();
        }
        
        private void Systems()
        {
            Container.Bind<ISystem>().To<MoveSystem>().AsSingle();
            Container.Bind<ISystem>().To<StartSpawnSystem>().AsSingle();
        }
    }
}