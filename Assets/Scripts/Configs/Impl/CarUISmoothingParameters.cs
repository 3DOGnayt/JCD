using Data.Helpers;
using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Car/" + nameof(CarUISmoothingParameters), fileName = nameof(CarUISmoothingParameters))]
    public class CarUISmoothingParameters : ScriptableObject, ICarUISmoothing
    {
        [SerializeField] private CarSmoothingSetup carSmoothingSetup;

        public CarSmoothingSetup SmoothingSetup => carSmoothingSetup;
    }
}