using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Car/" + nameof(CarParameters), fileName = nameof(CarParameters))]
    public class CarParameters : ScriptableObject, ICarParameters
    {
        [SerializeField] private CarMovementParameters _carMovementParameters;
        [SerializeField] private CarSpeedsPresetParameters _carSpeedsPresetParameters;
        [SerializeField] private CarSlipParameters _carSlipParameters;

        public CarMovementParameters MovementParameters => _carMovementParameters;
        public CarSpeedsPresetParameters CarSpeedsPresetParameters => _carSpeedsPresetParameters;
        public CarSlipParameters SlipParameters => _carSlipParameters;
    }
}