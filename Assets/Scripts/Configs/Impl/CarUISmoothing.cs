using Data.Helpers;
using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Car/" + nameof(CarUISmoothing), fileName = nameof(CarUISmoothing))]
    public class CarUISmoothing : ScriptableObject, ICarUISmoothing
    {
        [SerializeField] private CarSmoothingSettings _carSmoothingSettings;

        public CarSmoothingSettings SmoothingSettings => _carSmoothingSettings;
    }
}