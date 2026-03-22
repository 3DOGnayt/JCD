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
        [SerializeField] private Button _resetButton;
        [SerializeField] private float _forceToUpCar;

        private GameObject _car;

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
            _resetButton.onClick.AddListener(Reset);
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
            _car.transform.position += Vector3.up * _forceToUpCar;
        }
    }
}