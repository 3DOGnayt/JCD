using Scellecs.Morpeh;
using Systems.Car;
using Systems.MiniMapCamera;
using Systems.Race;
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
            Container.Bind<ISystem>().To<MapSpawnSystem>().AsSingle();
            Container.Bind<ISystem>().To<PlayerSpawnSystem>().AsSingle();
            Container.Bind<ISystem>().To<RaceLapSystem>().AsSingle();
            
            Container.Bind<ISystem>().To<InputSystem>().AsSingle();
            Container.Bind<ISystem>().To<MinimapSpawnSystem>().AsSingle();
            Container.Bind<ISystem>().To<MinimapFollowSystem>().AsSingle();
            
            Container.Bind<IFixedSystem>().To<HorizontalInputSystem>().AsSingle();

            // base
            Container.Bind<IFixedSystem>().To<BrakeSystem>().AsSingle();
            Container.Bind<IFixedSystem>().To<WheelDriveSystem>().AsSingle();
            Container.Bind<IFixedSystem>().To<PhysicsSpeedSystem>().AsSingle();
            Container.Bind<IFixedSystem>().To<CarAirControlSystem>().AsSingle();
            Container.Bind<IFixedSystem>().To<GearShiftSystem>().AsSingle();
            Container.Bind<IFixedSystem>().To<RPMSystem>().AsSingle();
            
            // feature
            Container.Bind<IFixedSystem>().To<SlipSystem>().AsSingle();
            Container.Bind<IFixedSystem>().To<DriftCorrectionSystem>().AsSingle();
            
            //Container.Bind<IFixedSystem>().To<EffectSystem>().AsSingle(); // TODO: plan B
            Container.Bind<IFixedSystem>().To<SkidmarksSystem>().AsSingle();
            Container.Bind<IFixedSystem>().To<SkidAudioSystem>().AsSingle();
            Container.Bind<IFixedSystem>().To<SkidSmokeSystem>().AsSingle();
            
            Container.Bind<IFixedSystem>().To<CrashEffectSystem>().AsSingle();
            Container.Bind<IFixedSystem>().To<CarLightsSystem>().AsSingle();
        }
    }
}