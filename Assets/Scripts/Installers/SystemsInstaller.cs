using Scellecs.Morpeh;
using Systems;
using Systems.Car;
using Systems.Spawn;
using Zenject;

namespace Installers
{
    public class SystemsInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Systems();
        }
       
        private void Systems()
        {
            Container.Bind<ISystem>().To<CameraSpawnSystem>().AsSingle();
            Container.Bind<ISystem>().To<CanvasSpawnSystem>().AsSingle();
            Container.Bind<ISystem>().To<PlayerSpawnSystem>().AsSingle();
            
            Container.Bind<ISystem>().To<InputSystem>().AsSingle();
            Container.Bind<IFixedSystem>().To<VerticalInputSystem>().AsSingle();
            Container.Bind<IFixedSystem>().To<HorizontalInputSystem>().AsSingle();
        }
    }
}