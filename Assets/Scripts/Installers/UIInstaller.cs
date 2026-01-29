using Core;
using Zenject;

namespace Installers
{
    public class UIInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<UIBootstrap>().AsSingle().NonLazy();
        }
    }
}
