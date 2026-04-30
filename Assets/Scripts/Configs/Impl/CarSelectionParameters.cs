using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Game/SelectionParameters/" + nameof(CarSelectionParameters), fileName = nameof(CarSelectionParameters), order = 1)]
    public partial class CarSelectionParameters : ScriptableObject
    {
        [SerializeField] private CarPresetParameters _selectedCar;
        [SerializeField] private CarMovementParameters _selectedCarMovementParameters;
        [SerializeField] private CarSpeedsPresetParameters _selectedCarSpeedsPresetParameters;
        [SerializeField] private CarSlipParameters _selectedCarSlipParameters;
        [SerializeField] private CarEngineAudioParameters _selectedCarEngineAudioParameters;
        [SerializeField] private int _selectedCarIndex;

        public CarPresetParameters SelectedCar => _selectedCar;
        public CarMovementParameters MovementParameters => _selectedCarMovementParameters;
        public CarSpeedsPresetParameters CarSpeedsPresetParameters => _selectedCarSpeedsPresetParameters;
        public CarSlipParameters SlipParameters => _selectedCarSlipParameters;
        public CarEngineAudioParameters SelectedCarEngineAudioParameters => _selectedCarEngineAudioParameters;
        public int SelectedCarIndex => _selectedCarIndex;

        public void SetSelectedCar(
            CarPresetParameters car,
            CarMovementParameters movementParameters,
            CarSpeedsPresetParameters speedsPresetParameters,
            CarSlipParameters slipParameters,
            CarEngineAudioParameters engineAudioParameters,
            int index
        )
        {
            _selectedCar = car;
            _selectedCarMovementParameters = movementParameters;
            _selectedCarSpeedsPresetParameters = speedsPresetParameters;
            _selectedCarSlipParameters = slipParameters;
            _selectedCarEngineAudioParameters = engineAudioParameters;
            _selectedCarIndex = index;
        }
    }
}