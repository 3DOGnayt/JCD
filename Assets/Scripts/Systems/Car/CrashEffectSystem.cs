using Components;
using Scellecs.Morpeh;
using Services;
using Views;
using Zenject;

namespace Systems.Car
{
    public sealed class CrashEffectSystem : IFixedSystem
    {
        [Inject] public World World { get; set; }
        [Inject] private ICrashEffectService _crashEffectService;

        private Filter _cars;
        private Stash<CarViewComponent> _carViewStash;

        public void OnAwake()
        {
            _cars = World.Filter
                .With<CarViewComponent>()
                .Build();

            _carViewStash = World.GetStash<CarViewComponent>();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var car in _cars)
            {
                ref var carViewComp = ref _carViewStash.Get(car);
                var carView = carViewComp.Value;
                if (carView == null)
                    continue;

                if (!(carView is IEffectsView effectsView))
                    continue;

                var listener = effectsView.CollisionListener;
                if (listener == null)
                    continue;

                var rb = carView.CarRigidbody;
                if (rb == null)
                    continue;

                var hasContact = listener.HasCollision;
                var point = listener.Point;

                _crashEffectService.UpdateCrash(rb, hasContact, point);

                listener.Clear();
            }
        }

        public void Dispose() { }
    }
}