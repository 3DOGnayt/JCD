using Components;
using Core;
using Scellecs.Morpeh;
using Zenject;

namespace Systems.Car.Test_Arcade
{
    public sealed class SmokeEffectSystem_A : IFixedSystem
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
                ref var carViewComponent = ref _carViewStash.Get(car);
                var skidNow = _skidmarksStash.Get(car).Value;

                var effectsView = carViewComponent.Value as IEffectsView;
                if (effectsView == null)
                    continue;

                var effects = effectsView.CarEffects;
                if (effects == null || effects.SmokeWheels == null)
                    continue;

                foreach (var smoke in effects.SmokeWheels)
                {
                    if (smoke == null)
                        continue;

                    smoke.SetActive(skidNow);
                }
            }
        }

        public void Dispose() { }
    }
}