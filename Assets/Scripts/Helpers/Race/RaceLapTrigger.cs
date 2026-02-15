using UnityEngine;

namespace Helpers.Race
{
    [RequireComponent(typeof(Collider))]
    public class RaceLapTrigger : MonoBehaviour
    {
        [SerializeField] private RaceLapTriggerManager _manager;

        private int _checkpointIndex = -1;

        public void Configure(RaceLapTriggerManager manager, int checkpointIndex)
        {
            _manager = manager;
            _checkpointIndex = checkpointIndex;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_manager == null || _checkpointIndex < 0 || other == null)
                return;

            var carView = other.GetComponentInParent<CarView.Impl.CarView>();
            if (carView == null)
                return;

            _manager.RegisterCheckpoint(_checkpointIndex);
        }
    }
}
