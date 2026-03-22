using Data.HelperClass;
using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Car/" + nameof(CarPresetParameters), fileName = nameof(CarPresetParameters))]
    public class CarPresetParameters : ScriptableObject, ICarPreset
    {
        [SerializeField] private GameObject _car;
        [Header("CAR PARAMETERS")]
        [Space]
        [SerializeField] private CarMassSetup carMassSetup;
        [Space]
        [Header("WHEELS")]
        [Space]
        [SerializeField] private WheelSetup frontWheelSetup;
        [Space]
        [SerializeField] private WheelSetup backWheelSetup;

        public GameObject Car => _car;
        public CarMassSetup CarMassSetup => carMassSetup;
        public WheelSetup FrontWheelSetup => frontWheelSetup;
        public WheelSetup BackWheelSetup => backWheelSetup;
    }
}