using Signals;
using UnityEngine;
using Zenject;

namespace UI.Helpers
{
    public class RaceHudVisibilityController : MonoBehaviour
    {
        [SerializeField] private GameObject _buttonsContainer;
        [SerializeField] private GameObject _parametersContainer;
        [SerializeField] private GameObject _speedometer;

        [Inject] private SignalBus _signalBus;

        private void Awake()
        {
            SetHudVisible(false);
        }

        private void OnEnable()
        {
            if (_signalBus != null)
                _signalBus.Subscribe<PlayerSpawnedSignal>(OnPlayerSpawned);
        }

        private void OnDisable()
        {
            if (_signalBus != null)
                _signalBus.Unsubscribe<PlayerSpawnedSignal>(OnPlayerSpawned);
        }

        private void OnPlayerSpawned(PlayerSpawnedSignal signal)
        {
            SetHudVisible(true);
        }

        private void SetHudVisible(bool isVisible)
        {
            if (_buttonsContainer != null)
                _buttonsContainer.SetActive(isVisible);
            if (_parametersContainer != null)
                _parametersContainer.SetActive(isVisible);
            if (_speedometer != null)
                _speedometer.SetActive(isVisible);
        }
    }
}
