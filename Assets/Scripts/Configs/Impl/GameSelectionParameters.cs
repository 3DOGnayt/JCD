using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Game/" + nameof(GameSelectionParameters), fileName = nameof(GameSelectionParameters), order = 1)]
    public class GameSelectionParameters : ScriptableObject
    {
        [SerializeField] private CarPresetParameters _selectedCar;
        [SerializeField] private CarParameters _selectedCarParameters;
        [SerializeField] private int _selectedCarIndex;
        [Space]
        [SerializeField] private GameObject _selectedMapPrefab;
        [SerializeField] private int _selectedMapIndex;
        [Space]
        [SerializeField] private string _selectedOpponentName;
        [SerializeField] private float _selectedOpponentDifficulty;
        [SerializeField] private int _selectedOpponentIndex;

        public CarPresetParameters SelectedCar => _selectedCar;
        public CarParameters SelectedCarParameters => _selectedCarParameters;
        public int SelectedCarIndex => _selectedCarIndex;
        
        public GameObject SelectedMapPrefab => _selectedMapPrefab;
        public int SelectedMapIndex => _selectedMapIndex;
        
        public string SelectedOpponentName => _selectedOpponentName;
        public float SelectedOpponentDifficulty => _selectedOpponentDifficulty;
        public int SelectedOpponentIndex => _selectedOpponentIndex;

        public void SetSelectedCar(CarPresetParameters car, CarParameters parameters, int index)
        {
            _selectedCar = car;
            _selectedCarParameters = parameters;
            _selectedCarIndex = index;
        }

        public void SetSelectedMap(GameObject mapPrefab, int index)
        {
            _selectedMapPrefab = mapPrefab;
            _selectedMapIndex = index;
        }

        public void SetSelectedOpponent(string opponentName, float difficulty, int index)
        {
            _selectedOpponentName = opponentName;
            _selectedOpponentDifficulty = difficulty;
            _selectedOpponentIndex = index;
        }
    }
}