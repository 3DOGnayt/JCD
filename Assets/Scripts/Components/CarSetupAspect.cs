using Scellecs.Morpeh;

namespace Components
{
    public struct CarSetupAspect : IAspect, IFilterExtension
    {
        public Entity Entity { get; set; }
        
        private Stash<SpeedComponent> speed;
        private Stash<GearboxComponent> gear;
        private Stash<BackSpeedComponent> backSpeed;
        private Stash<EngineRpmComponent> engineRpm;
        private Stash<AccelerationMultiplierComponent> accelerationMultiplier;
        private Stash<DecelerationMultiplierComponent> decelerationMultiplier;
        private Stash<SteeringAngleComponent> steeringAngle;
        private Stash<SteeringSpeedComponent> steeringSpeed;
        private Stash<BrakeForceComponent> brakeForce;
        private Stash<HandbrakeInputComponent> handbrakeInput;
        private Stash<DriftMultiplierComponent> driftMultiplier;
    
        public ref SpeedComponent Speed => ref speed.Get(Entity);
        public ref GearboxComponent Gearbox => ref gear.Get(Entity);
        public ref BackSpeedComponent BackSpeed => ref backSpeed.Get(Entity);
        public ref EngineRpmComponent EngineRpm => ref engineRpm.Get(Entity);
        public ref AccelerationMultiplierComponent AccelerationMultiplier => ref accelerationMultiplier.Get(Entity);
        public ref DecelerationMultiplierComponent DecelerationMultiplier => ref decelerationMultiplier.Get(Entity);
        public ref SteeringAngleComponent SteeringAngle => ref steeringAngle.Get(Entity);
        public ref SteeringSpeedComponent SteeringSpeed => ref steeringSpeed.Get(Entity);
        public ref BrakeForceComponent BrakeForce => ref brakeForce.Get(Entity);
        public ref HandbrakeInputComponent HandbrakeInput => ref handbrakeInput.Get(Entity);
        public ref DriftMultiplierComponent DriftMultiplier => ref driftMultiplier.Get(Entity);

        public void OnGetAspectFactory(World world)
        {
            speed = world.GetStash<SpeedComponent>();
            gear = world.GetStash<GearboxComponent>();
            backSpeed = world.GetStash<BackSpeedComponent>();
            engineRpm = world.GetStash<EngineRpmComponent>();
            accelerationMultiplier = world.GetStash<AccelerationMultiplierComponent>();
            decelerationMultiplier = world.GetStash<DecelerationMultiplierComponent>();
            steeringAngle = world.GetStash<SteeringAngleComponent>();
            steeringSpeed = world.GetStash<SteeringSpeedComponent>();
            brakeForce = world.GetStash<BrakeForceComponent>();
            handbrakeInput = world.GetStash<HandbrakeInputComponent>();
            driftMultiplier = world.GetStash<DriftMultiplierComponent>();
        }
        
        public FilterBuilder Extend(FilterBuilder rootFilter) => rootFilter
            .With<SpeedComponent>()
            .With<GearboxComponent>()
            .With<BackSpeedComponent>()
            .With<EngineRpmComponent>()
            .With<AccelerationMultiplierComponent>()
            .With<DecelerationMultiplierComponent>()
            .With<SteeringAngleComponent>()
            .With<SteeringSpeedComponent>()
            .With<BrakeForceComponent>()
            .With<HandbrakeInputComponent>()
            .With<DriftMultiplierComponent>()
        ;

    }
}