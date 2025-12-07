using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Car/" + nameof(CarParameters), fileName = nameof(CarParameters))]
    public class CarParameters : ScriptableObject, ICarParameters
    {
        [SerializeField] private CarMovementParameters _carMovementParameters;
        [SerializeField] private SpeedsPreset _speedsPreset;

        public CarMovementParameters MovementParameters => _carMovementParameters;
        public SpeedsPreset SpeedsPreset => _speedsPreset;
    }
}