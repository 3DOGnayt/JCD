using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Game/SelectionParameters/" + nameof(CarSelectionParameters), fileName = nameof(CarSelectionParameters), order = 1)]
    public class CarSelectionParameters : ScriptableObject
    {
        [SerializeField] private CarPresetParameters _selectedCar;
        [SerializeField] private CarParameters _selectedCarParameters;
        [SerializeField] private int _selectedCarIndex;

        public CarPresetParameters SelectedCar => _selectedCar;
        public CarParameters SelectedCarParameters => _selectedCarParameters;
        public int SelectedCarIndex => _selectedCarIndex;

        public void SetSelectedCar(CarPresetParameters car, CarParameters parameters, int index)
        {
            _selectedCar = car;
            _selectedCarParameters = parameters;
            _selectedCarIndex = index;
        }
    }
}