using Scellecs.Morpeh;
using Systems;
using Systems.Car;
using Systems.Car.Test_Arcade;
using Systems.Car.Test_Manual;
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
            //Container.Bind<IFixedSystem>().To<VerticalInputSystem>().AsSingle(); // mb later
            //Container.Bind<IFixedSystem>().To<SpeedSystem>().AsSingle(); // mb later
            Container.Bind<IFixedSystem>().To<HorizontalInputSystem>().AsSingle();
            //Container.Bind<IFixedSystem>().To<RPMSystem>().AsSingle();
            //Container.Bind<IFixedSystem>().To<SpeedSystem>().AsSingle();
            //Container.Bind<IFixedSystem>().To<PhysicsSpeedSystem>().AsSingle();
            //Container.Bind<IFixedSystem>().To<GearShiftSystem>().AsSingle();
            //Container.Bind<IFixedSystem>().To<WheelDriveSystem>().AsSingle();
            
            //tests
            // до основных систем рабочий варик более менее
            //Container.Bind<IFixedSystem>().To<RPMSystem_Test>().AsSingle();
            //Container.Bind<IFixedSystem>().To<GearShiftSystem_Test>().AsSingle();
            //Container.Bind<IFixedSystem>().To<WheelDriveSystem_Test>().AsSingle();
            //Container.Bind<IFixedSystem>().To<PhysicsSpeedSystem_Test>().AsSingle();
            
            //Container.Bind<IFixedSystem>().To<RPMSystem_M>().AsSingle();
            //Container.Bind<IFixedSystem>().To<GearShiftSystem_M>().AsSingle();
            //Container.Bind<IFixedSystem>().To<WheelDriveSystem_M>().AsSingle();
            //Container.Bind<IFixedSystem>().To<PhysicsSpeedSystem_M>().AsSingle();

            Container.Bind<IFixedSystem>().To<WheelDriveSystem_A>().AsSingle();
            Container.Bind<IFixedSystem>().To<PhysicsSpeedSystem_A>().AsSingle();
            Container.Bind<IFixedSystem>().To<GearShiftSystem_A>().AsSingle();
            Container.Bind<IFixedSystem>().To<RPMSystem_A>().AsSingle();
            Container.Bind<IFixedSystem>().To<SlipSystem_A>().AsSingle();
            
            Container.Bind<IFixedSystem>().To<EffectSystem_A>().AsSingle();
        }
    }
}