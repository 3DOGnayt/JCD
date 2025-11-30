using Services.Impl;
using Zenject;

namespace Installers
{
    public class ServicesInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Services();
        }

        private void Services()
        {
            Container.BindInterfacesAndSelfTo<InputService>().AsSingle();
        }
    }
}