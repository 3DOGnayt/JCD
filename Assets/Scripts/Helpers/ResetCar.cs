using Helpers.CarView;
using Services;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Helpers
{
    public class ResetCar : MonoBehaviour
    {
        public Button ResetButton;

        public GameObject _car;
        public float ForceToUpCar;

        [Inject]
        public void Construct(IEventService eventService)
        {
            eventService.PlayerSpawnedStream.Subscribe(OnPlayerSpawned).AddTo(this);
        }

        private void OnPlayerSpawned(ICarView carView)
        {
            _car = carView.CarTransform.gameObject;
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
