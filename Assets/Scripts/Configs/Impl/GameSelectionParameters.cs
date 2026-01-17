using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Game/GameSelection", fileName = "GameSelection")]
    public class GameSelectionParameters : ScriptableObject
    {
        [SerializeField] private CarPresetParameters _selectedCar;
        [SerializeField] private CarParameters _selectedCarParameters;
        [SerializeField] private GameObject _selectedMapPrefab;
        [SerializeField] private int _selectedCarIndex;
        [SerializeField] private int _selectedMapIndex;

        public CarPresetParameters SelectedCar => _selectedCar;
        public CarParameters SelectedCarParameters => _selectedCarParameters;
        public GameObject SelectedMapPrefab => _selectedMapPrefab;
        public int SelectedCarIndex => _selectedCarIndex;
        public int SelectedMapIndex => _selectedMapIndex;

        public void SetSelectedCar(CarPresetParameters car, CarParameters parameters, int index)
        {
            _selectedCar = car;
            _selectedCarParameters = parameters;
            _selectedCarIndex = index;
        }

        public void SetSelectedCar(CarPresetParameters car, int index)
        {
            SetSelectedCar(car, null, index);
        }

        public void SetSelectedMap(GameObject mapPrefab, int index)
        {
            _selectedMapPrefab = mapPrefab;
            _selectedMapIndex = index;
        }
    }
}
