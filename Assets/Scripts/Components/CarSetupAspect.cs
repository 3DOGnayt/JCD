using Scellecs.Morpeh;

namespace Components
{
    public struct CarSetupAspect : IAspect, IFilterExtension
    {
        public Entity Entity { get; set; }
        
        private Stash<SpeedComponent> speed;
        private Stash<CurrentGearComponent> currentGear;
        private Stash<GearCountComponent> gearCount;
        private Stash<BackSpeedComponent> backSpeed;
        private Stash<MotorTorqueComponent> motorTorque;
        private Stash<AccelerationMultiplierComponent> accelerationMultiplier;
        private Stash<DecelerationMultiplierComponent> decelerationMultiplier;
        private Stash<SteeringAngleComponent> steeringAngle;
        private Stash<SteeringSpeedComponent> steeringSpeed;
        private Stash<BrakeForceComponent> brakeForce;
        private Stash<DriftMultiplierComponent> driftMultiplier;
    
        public ref SpeedComponent Speed => ref speed.Get(Entity);
        public ref CurrentGearComponent CurrentGear => ref currentGear.Get(Entity);
        public ref GearCountComponent GearCount => ref gearCount.Get(Entity);
        public ref BackSpeedComponent BackSpeed => ref backSpeed.Get(Entity);
        public ref MotorTorqueComponent MotorTorque => ref motorTorque.Get(Entity);
        public ref AccelerationMultiplierComponent AccelerationMultiplier => ref accelerationMultiplier.Get(Entity);
        public ref DecelerationMultiplierComponent DecelerationMultiplier => ref decelerationMultiplier.Get(Entity);
        public ref SteeringAngleComponent SteeringAngle => ref steeringAngle.Get(Entity);
        public ref SteeringSpeedComponent SteeringSpeed => ref steeringSpeed.Get(Entity);
        public ref BrakeForceComponent BrakeForce => ref brakeForce.Get(Entity);
        public ref DriftMultiplierComponent DriftMultiplier => ref driftMultiplier.Get(Entity);

        public void OnGetAspectFactory(World world)
        {
            speed = world.GetStash<SpeedComponent>();
            currentGear = world.GetStash<CurrentGearComponent>();
            gearCount = world.GetStash<GearCountComponent>();
            backSpeed = world.GetStash<BackSpeedComponent>();
            motorTorque = world.GetStash<MotorTorqueComponent>();
            accelerationMultiplier = world.GetStash<AccelerationMultiplierComponent>();
            decelerationMultiplier = world.GetStash<DecelerationMultiplierComponent>();
            steeringAngle = world.GetStash<SteeringAngleComponent>();
            steeringSpeed = world.GetStash<SteeringSpeedComponent>();
            brakeForce = world.GetStash<BrakeForceComponent>();
            driftMultiplier = world.GetStash<DriftMultiplierComponent>();
        }
        
        public FilterBuilder Extend(FilterBuilder rootFilter) => rootFilter
            .With<SpeedComponent>()
            .With<CurrentGearComponent>()
            .With<GearCountComponent>()
            .With<BackSpeedComponent>()
            .With<MotorTorqueComponent>()
            .With<AccelerationMultiplierComponent>()
            .With<DecelerationMultiplierComponent>()
            .With<SteeringAngleComponent>()
            .With<SteeringSpeedComponent>()
            .With<BrakeForceComponent>()
            .With<DriftMultiplierComponent>()
        ;

    }
}