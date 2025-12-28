using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Car/" + nameof(CarParameters), fileName = nameof(CarParameters))]
    public class CarParameters : ScriptableObject, ICarParameters
    {
        [SerializeField] private CarMovementParameters _carMovementParameters;
        [SerializeField] private SpeedsPresetParameters _speedsPresetParameters;
        [SerializeField] private CarSlipParameters _carSlipParameters;

        public CarMovementParameters MovementParameters => _carMovementParameters;
        public SpeedsPresetParameters SpeedsPresetParameters => _speedsPresetParameters;
        public CarSlipParameters SlipParameters => _carSlipParameters;
    }
}