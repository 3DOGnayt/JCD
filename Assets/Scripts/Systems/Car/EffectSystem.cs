using Components;
using Scellecs.Morpeh;
using Views;
using Zenject;

namespace Systems.Car
{
    //TODO: plan B
    public class EffectSystem : IFixedSystem
    {
        [Inject] public World World { get; set; }

        private Filter _cars;
        private Stash<CarViewComponent> _carViewStash;
        private Stash<SkidmarksComponent> _skidmarksStash;

        public void OnAwake()
        {
            _cars = World.Filter
                .With<CarViewComponent>()
                .With<SkidmarksComponent>()
                .Build();

            _carViewStash = World.GetStash<CarViewComponent>();
            _skidmarksStash = World.GetStash<SkidmarksComponent>();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var car in _cars)
            {
                var carView = _carViewStash.Get(car).Value;
                if (carView is not IEffectsView trailView)
                    continue;

                ref var mark = ref _skidmarksStash.Get(car).Value;

                var traces = trailView.CarEffectsSetup.TracesWheels;
                if (traces == null)
                    continue;

                var emitting = mark;

                foreach (var trail in traces)
                {
                    if (trail == null)
                        continue;

                    trail.emitting = emitting;
                }
            }
        }

        public void Dispose() { }
    }
}