using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Game/SelectionParameters/" + nameof(CarSelectionParameters), fileName = nameof(CarSelectionParameters), order = 1)]
    public class CarSelectionParameters : ScriptableObject
    {
        [SerializeField] private CarPresetParameters _selectedCar;
        [SerializeField] private CarParameters _selectedCarParameters;
        [SerializeField] private CarEngineAudioParameters _selectedCarEngineAudioParameters;
        [SerializeField] private int _selectedCarIndex;

        public CarPresetParameters SelectedCar => _selectedCar;
        public CarParameters SelectedCarParameters => _selectedCarParameters;
        public CarEngineAudioParameters SelectedCarEngineAudioParameters => _selectedCarEngineAudioParameters;
        public int SelectedCarIndex => _selectedCarIndex;

        public void SetSelectedCar(
            CarPresetParameters car,
            CarParameters parameters,
            CarEngineAudioParameters engineAudioParameters,
            int index)
        {
            _selectedCar = car;
            _selectedCarParameters = parameters;
            _selectedCarEngineAudioParameters = engineAudioParameters;
            _selectedCarIndex = index;
        }
    }
}