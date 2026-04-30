using Components;
using Helpers.Car.Impl;
using Scellecs.Morpeh;
using UnityEngine;
using Zenject;

namespace Helpers.Race
{
    [RequireComponent(typeof(Collider))]
    public class RaceLapTrigger : MonoBehaviour
    {
        [Inject] private World _world;

        private int _checkpointIndex = -1;
        private Filter _carFilter;
        private Stash<CarViewComponent> _carViewStash;
        private Stash<PlayerTagComponent> _playerTagStash;
        private Stash<OpponentTagComponent> _opponentTagStash;

        public void Configure(int checkpointIndex)
        {
            _checkpointIndex = checkpointIndex;
        }

        private void Start()
        {
            if (_world == null)
                return;

            _carFilter = _world.Filter
                .With<CarViewComponent>()
                .Build();

            _carViewStash = _world.GetStash<CarViewComponent>();
            _playerTagStash = _world.GetStash<PlayerTagComponent>();
            _opponentTagStash = _world.GetStash<OpponentTagComponent>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_world == null || _checkpointIndex < 0 || other == null)
                return;

            var carView = other.GetComponentInParent<CarView>();
            if (carView == null)
                return;

            if (!TryResolveCarEntity(carView, out var carEntity))
                return;

            var entity = _world.CreateEntity();
            if (_playerTagStash.Has(carEntity) || _opponentTagStash.Has(carEntity))
            {
                entity.SetComponent(new RaceLapTriggerEventComponent
                {
                    CheckpointIndex = _checkpointIndex,
                    CarEntity = carEntity
                });
            }
        }

        private bool TryResolveCarEntity(CarView carView, out Entity carEntity)
        {
            carEntity = default;

            if (_carFilter == null || _carViewStash == null)
                return false;

            foreach (var entity in _carFilter)
            {
                var view = _carViewStash.Get(entity).Value;
                if (view == null || !ReferenceEquals(view, carView))
                    continue;

                carEntity = entity;
                return true;
            }

            return false;
        }
    }
}