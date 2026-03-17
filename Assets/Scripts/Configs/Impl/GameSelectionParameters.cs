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

        public int SelectedCarIndex => _carSelectionParameters != null ? _carSelectionParameters.SelectedCarIndex : 0;

        public int SelectedMapIndex => _mapSelectionParameters != null ? _mapSelectionParameters.SelectedMapIndex : 0;
        public EMap SelectedMap => _mapSelectionParameters != null ? _mapSelectionParameters.SelectedMap : EMap.None;

        public EGameMod GameMod => _gameModeSelectionParameters != null ? _gameModeSelectionParameters.GameMod : EGameMod.None;

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

    }
}