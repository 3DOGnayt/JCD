using Data.Enums;
using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Game/" + nameof(GameSelectionParameters), fileName = nameof(GameSelectionParameters), order = 1)]
    public class GameSelectionParameters : ScriptableObject
    {
        [SerializeField] private CarSelectionParameters _carSelectionParameters;
        [SerializeField] private MapSelectionParameters _mapSelectionParameters;
        [SerializeField] private GameModeSelectionParameters _gameModeSelectionParameters;
        [SerializeField] private OpponentSelectionParameters _opponentSelectionParameters;
        [SerializeField] private AudioSelectionParameters _audioSelectionParameters;

        public CarPresetParameters SelectedCar => _carSelectionParameters != null ? _carSelectionParameters.SelectedCar : null;
        public CarParameters SelectedCarParameters => _carSelectionParameters != null ? _carSelectionParameters.SelectedCarParameters : null;
        public int SelectedCarIndex => _carSelectionParameters != null ? _carSelectionParameters.SelectedCarIndex : 0;

        public GameObject SelectedMapPrefab => _mapSelectionParameters != null ? _mapSelectionParameters.SelectedMapPrefab : null;
        public int SelectedMapIndex => _mapSelectionParameters != null ? _mapSelectionParameters.SelectedMapIndex : 0;
        public int SelectedMapSelectionCount => _mapSelectionParameters != null ? _mapSelectionParameters.SelectedMapSelectionCount : 0;
        public int SelectedMapLapCount => _mapSelectionParameters != null ? _mapSelectionParameters.SelectedMapLapCount : 0;
        public EMap SelectedMap => _mapSelectionParameters != null ? _mapSelectionParameters.SelectedMap : EMap.None;

        public EGameMod GameMod => _gameModeSelectionParameters != null ? _gameModeSelectionParameters.GameMod : EGameMod.None;

        public string SelectedOpponentName => _opponentSelectionParameters != null ? _opponentSelectionParameters.SelectedOpponentName : string.Empty;
        public float SelectedOpponentDifficulty => _opponentSelectionParameters != null ? _opponentSelectionParameters.SelectedOpponentDifficulty : 0f;
        public int SelectedOpponentIndex => _opponentSelectionParameters != null ? _opponentSelectionParameters.SelectedOpponentIndex : 0;

        public EAudioSubType SelectedMusicSubType => _audioSelectionParameters != null ? _audioSelectionParameters.SelectedMusicSubType : EAudioSubType.None;
        public int SelectedMusicIndex => _audioSelectionParameters != null ? _audioSelectionParameters.SelectedMusicIndex : 0;

        public void SetSources(
            CarSelectionParameters carSelectionParameters,
            MapSelectionParameters mapSelectionParameters,
            GameModeSelectionParameters gameModeSelectionParameters,
            OpponentSelectionParameters opponentSelectionParameters,
            AudioSelectionParameters audioSelectionParameters)
        {
            _carSelectionParameters = carSelectionParameters;
            _mapSelectionParameters = mapSelectionParameters;
            _gameModeSelectionParameters = gameModeSelectionParameters;
            _opponentSelectionParameters = opponentSelectionParameters;
            _audioSelectionParameters = audioSelectionParameters;
        }

        public void SetSelectedCar(CarPresetParameters car, CarParameters parameters, int index)
        {
            if (_carSelectionParameters == null)
                return;

            _carSelectionParameters.SetSelectedCar(car, parameters, index);
        }

        public void SetSelectedMap(GameObject mapPrefab, int index, EMap map, int selectionCount, int lapCount)
        {
            if (_mapSelectionParameters == null)
                return;

            _mapSelectionParameters.SetSelectedMap(mapPrefab, index, map, selectionCount, lapCount);
        }

        public void SetSelectedGameMode(EGameMod gameMod)
        {
            if (_gameModeSelectionParameters == null)
                return;

            _gameModeSelectionParameters.SetSelectedGameMode(gameMod);
        }

        public void SetSelectedOpponent(string opponentName, float difficulty, int index)
        {
            if (_opponentSelectionParameters == null)
                return;

            _opponentSelectionParameters.SetSelectedOpponent(opponentName, difficulty, index);
        }

        public void SetSelectedMusic(EAudioSubType subType, int index)
        {
            if (_audioSelectionParameters == null)
                return;

            _audioSelectionParameters.SetSelectedMusic(subType, index);
        }
    }
}