using Core;
using Scellecs.Morpeh;
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
            Container.Bind<World>().FromMethod(_ => World.Create()).AsSingle();
            Container.Bind<Startup>().FromInstance(startup).AsSingle();
        }
    }
}