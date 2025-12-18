using System.Collections.Generic;
using Components;
using Core;
using Scellecs.Morpeh;
using UnityEngine;
using Zenject;

namespace Systems.Car.Test_Arcade
{
    public sealed class SmokeEffectSystem_A : IFixedSystem
    {
        [Inject] public World World { get; set; }

        private Filter _cars;
        private Stash<CarViewComponent> _carViewStash;
        private Stash<SkidmarksComponent> _skidmarksStash;

        private readonly Dictionary<ParticleSystem, bool> _wasSkidding = new();

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
                bool skidNow = _skidmarksStash.Get(car).Value;

                var trailView = carViewComponent.Value as IEffectsView;
                if (trailView == null)
                    continue;

                var effects = trailView.CarEffects;
                if (effects == null || effects.SmokeWheels == null)
                    continue;

                foreach (var smoke in effects.SmokeWheels)
                {
                    if (smoke == null)
                        continue;

                    UpdateSmoke(smoke, skidNow);
                }
            }
        }

        private void UpdateSmoke(ParticleSystem ps, bool skidNow)
        {
            bool wasSkid = false;
            _wasSkidding.TryGetValue(ps, out wasSkid);

            if (skidNow && !wasSkid)
            {
                ps.Clear();
                ps.Play();
            }
            else if (!skidNow && wasSkid)
            {
                ps.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            }

            _wasSkidding[ps] = skidNow;
        }

        public void Dispose() { }
    }
}