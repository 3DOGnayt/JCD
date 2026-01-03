using Scellecs.Morpeh;
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
            Container.Bind<IFixedSystem>().To<HorizontalInputSystem>().AsSingle();
            
            // base
            Container.Bind<IFixedSystem>().To<WheelDriveSystem>().AsSingle();
            Container.Bind<IFixedSystem>().To<PhysicsSpeedSystem>().AsSingle();
            Container.Bind<IFixedSystem>().To<GearShiftSystem>().AsSingle();
            Container.Bind<IFixedSystem>().To<RPMSystem>().AsSingle();
            
            // feature
            Container.Bind<IFixedSystem>().To<SlipSystem>().AsSingle();
            
            //Container.Bind<IFixedSystem>().To<EffectSystem>().AsSingle(); // TODO: plan B
            Container.Bind<IFixedSystem>().To<SkidmarksSystem>().AsSingle();
            Container.Bind<IFixedSystem>().To<SkidSmokeSystem>().AsSingle();
            
            Container.Bind<IFixedSystem>().To<CrashEffectSystem>().AsSingle();
            Container.Bind<IFixedSystem>().To<CarLightsSystem>().AsSingle();
        }
    }
}
