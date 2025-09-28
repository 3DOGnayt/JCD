using Signals;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Tools
{
    public class ResetCar : MonoBehaviour
    {
        public Button ResetButton;

        public GameObject _car;
        public float ForceToUpCar;

        [Inject]
        public void Construct(SignalBus signalBus)
        {
            signalBus.Subscribe<PlayerSpawnedSignal>(OnPlayerSpawned);
        }

        private void OnPlayerSpawned(PlayerSpawnedSignal signal)
        {
            _car = signal.CarView.CarTransform.gameObject;
        }

        private void Awake()
        {
            ResetButton.onClick.AddListener(Reset);
        }

        private void Reset()
        {
            UpCar();
            RotateCar();
        }

        private void RotateCar()
        {
            var rotation = _car.transform.rotation;
            rotation = Quaternion.Euler(new Vector3(rotation.x, rotation.y, 0));
            _car.transform.rotation = rotation;
        }

        private void UpCar()
        {
            _car.transform.position += Vector3.up * ForceToUpCar;
        }
    }
}