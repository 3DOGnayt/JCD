using UnityEngine;
using UnityEngine.UI;

namespace Tools
{
    public class ResetCar : MonoBehaviour
    {
        public Button ResetButton;

        public GameObject _car;
        public float ForceToUpCar;

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