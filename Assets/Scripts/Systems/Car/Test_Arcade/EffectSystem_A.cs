using Components;
using Core;
using Scellecs.Morpeh;
using Zenject;

namespace Systems.Car.Test_Arcade
{
    public class EffectSystem_A : IFixedSystem
    {
        [Inject] public World World { get; set; }
        
        private Filter _cars;
        private Stash<CarViewComponent> _carViewStash;
        private Stash<HandbrakeInputComponent> _handbrakeStash;
        private Stash<DriftComponent> _driftStash;
        
        public void OnAwake()
        {
            _cars = World.Filter
                .With<CarViewComponent>()
                .Build();

            _carViewStash = World.GetStash<CarViewComponent>();
            _handbrakeStash = World.GetStash<HandbrakeInputComponent>();
            _driftStash = World.GetStash<DriftComponent>();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var car in _cars)
            {
                ref var carView = ref _carViewStash.Get(car).Value;
                ref var handbrake = ref _handbrakeStash.Get(car).Value;
                ref var drift = ref _driftStash.Get(car).Value;

                var trailsView = carView as ITrailView;
                
                if (drift)
                {
                    foreach (var tracesWheel in trailsView.CarEffects.TracesWheels)
                        tracesWheel.emitting = true;
                }
                else
                {
                    foreach (var tracesWheel in trailsView.CarEffects.TracesWheels)
                        tracesWheel.emitting = false;
                }
                
            }
        }

        public void Dispose() { }
    }
}