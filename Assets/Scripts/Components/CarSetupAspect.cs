using Scellecs.Morpeh;

namespace Components
{
    public struct CarSetupAspect : IAspect, IFilterExtension
    {
        public Entity Entity { get; set; }
        
        private Stash<SpeedComponent> speed;
        private Stash<GearComponent> gear;
        private Stash<BackSpeedComponent> backSpeed;
        private Stash<EngineRpmComponent> engineRpm;
        private Stash<BrakeInputComponent> brakeForce;
        private Stash<HandbrakeInputComponent> handbrakeInput;
        private Stash<DriftMultiplierComponent> driftMultiplier;
    
        public ref SpeedComponent Speed => ref speed.Get(Entity);
        public ref GearComponent Gear => ref gear.Get(Entity);
        public ref BackSpeedComponent BackSpeed => ref backSpeed.Get(Entity);
        public ref EngineRpmComponent EngineRpm => ref engineRpm.Get(Entity);
        public ref BrakeInputComponent BrakeInput => ref brakeForce.Get(Entity);
        public ref HandbrakeInputComponent HandbrakeInput => ref handbrakeInput.Get(Entity);
        public ref DriftMultiplierComponent DriftMultiplier => ref driftMultiplier.Get(Entity);

        public void OnGetAspectFactory(World world)
        {
            speed = world.GetStash<SpeedComponent>();
            gear = world.GetStash<GearComponent>();
            backSpeed = world.GetStash<BackSpeedComponent>();
            engineRpm = world.GetStash<EngineRpmComponent>();
            brakeForce = world.GetStash<BrakeInputComponent>();
            handbrakeInput = world.GetStash<HandbrakeInputComponent>();
            driftMultiplier = world.GetStash<DriftMultiplierComponent>();
        }
        
        public FilterBuilder Extend(FilterBuilder rootFilter) => rootFilter
            .With<SpeedComponent>()
            .With<GearComponent>()
            .With<BackSpeedComponent>()
            .With<EngineRpmComponent>()
            .With<BrakeInputComponent>()
            .With<HandbrakeInputComponent>()
            .With<DriftMultiplierComponent>()
        ;
    }
}