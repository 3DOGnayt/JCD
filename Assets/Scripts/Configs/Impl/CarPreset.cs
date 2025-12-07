using Data;
using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Car/" + nameof(CarPreset), fileName = nameof(CarPreset))]
    public class CarPreset : ScriptableObject, ICarPreset
    {
        [SerializeField] private GameObject _car;
        [Header("GAME CAR PARAMETERS")]
        [SerializeField] private CarSetup _carSetup;
        [Header("CAR PARAMETERS")]
        [Space]
        [SerializeField] private CarMassParameters carMassParameters;
        [Space]
        [Header("WHEELS")]
        [Space]
        [SerializeField] private WheelParameters _frontWheelParameters;
        [Space]
        [SerializeField] private WheelParameters _backWheelParameters;

        public GameObject Car => _car;
        public CarSetup CarSetup => _carSetup;
        public CarMassParameters CarMassParameters => carMassParameters;
        public WheelParameters FrontWheelParameters => _frontWheelParameters;
        public WheelParameters BackWheelParameters => _backWheelParameters;
    }
}