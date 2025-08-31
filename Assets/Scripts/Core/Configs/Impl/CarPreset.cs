using Core.Data;
using UnityEngine;

namespace Core.Configs.Impl
{
    [CreateAssetMenu(menuName = "Car/CarPreset", fileName = "CarPreset")]
    public class CarPreset : ScriptableObject, ICarPreset
    {
        [SerializeField] private GameObject _car;
        [Header("CAR PARAMETERS")]
        [Space]
        [SerializeField] private CarParameters _carParameters;
        [Space]
        [Header("WHEELS")]
        [Space]
        [SerializeField] private WheelParameters _wheelParameters;
        [SerializeField] private WheelSubParameters _wheelSubParameters;

        public GameObject Car => _car;
        public CarParameters CarParameters => _carParameters;
        public WheelParameters WheelParameters => _wheelParameters;
        public WheelSubParameters WheelSubParameters => _wheelSubParameters;
    }
}