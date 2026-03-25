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

        public void Configure(int checkpointIndex)
        {
            _checkpointIndex = checkpointIndex;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_world == null || _checkpointIndex < 0 || other == null)
                return;

            var carView = other.GetComponentInParent<CarView>();
            if (carView == null)
                return;

            var entity = _world.CreateEntity();
            entity.SetComponent(new RaceLapTriggerEventComponent { CheckpointIndex = _checkpointIndex });
        }
    }
}