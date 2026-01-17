using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Game/GameSelection", fileName = "GameSelection")]
    public class GameSelectionParameters : ScriptableObject
    {
        [SerializeField] private CarPresetParameters _selectedCar;
        [SerializeField] private GameObject _selectedMapPrefab;
        [SerializeField] private int _selectedCarIndex;
        [SerializeField] private int _selectedMapIndex;

        public CarPresetParameters SelectedCar => _selectedCar;
        public GameObject SelectedMapPrefab => _selectedMapPrefab;
        public int SelectedCarIndex => _selectedCarIndex;
        public int SelectedMapIndex => _selectedMapIndex;

        public void SetSelectedCar(CarPresetParameters car, int index)
        {
            _selectedCar = car;
            _selectedCarIndex = index;
        }

        public void SetSelectedMap(GameObject mapPrefab, int index)
        {
            _selectedMapPrefab = mapPrefab;
            _selectedMapIndex = index;
        }
    }
}
