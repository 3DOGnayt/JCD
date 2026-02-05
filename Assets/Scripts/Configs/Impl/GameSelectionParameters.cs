using Data.Enums;
using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Game/" + nameof(GameSelectionParameters), fileName = nameof(GameSelectionParameters), order = 1)]
    public class GameSelectionParameters : ScriptableObject
    {
        [Header("Selected Car")]
        [SerializeField] private CarPresetParameters _selectedCar;
        [SerializeField] private CarParameters _selectedCarParameters;
        [SerializeField] private int _selectedCarIndex;
        [Space]
        [Header("Selected Map")]
        [SerializeField] private GameObject _selectedMapPrefab;
        [SerializeField] private int _selectedMapIndex;
        [Space]
        [Header("Selected GameMod")]
        [SerializeField] private EGameMod _gameMod;
        [Space]
        [Header("Selected Opponent")]
        [SerializeField] private string _selectedOpponentName;
        [SerializeField] private float _selectedOpponentDifficulty;
        [SerializeField] private int _selectedOpponentIndex;

        public CarPresetParameters SelectedCar => _selectedCar;
        public CarParameters SelectedCarParameters => _selectedCarParameters;
        public int SelectedCarIndex => _selectedCarIndex;
        
        public GameObject SelectedMapPrefab => _selectedMapPrefab;
        public int SelectedMapIndex => _selectedMapIndex;

        public EGameMod GameMod => _gameMod;
        
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
        
        public void SetSelectedGameMode(EGameMod gameMod)
        {
            _gameMod = gameMod;
        }

        public void SetSelectedOpponent(string opponentName, float difficulty, int index)
        {
            _selectedOpponentName = opponentName;
            _selectedOpponentDifficulty = difficulty;
            _selectedOpponentIndex = index;
        }
    }
}