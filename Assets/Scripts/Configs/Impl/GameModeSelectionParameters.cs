using Data.Enums;
using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Game/SelectionParameters/" + nameof(GameModeSelectionParameters), fileName = nameof(GameModeSelectionParameters), order = 1)]
    public class GameModeSelectionParameters : ScriptableObject
    {
        [SerializeField] private EGameMod _gameMod;

        public EGameMod GameMod => _gameMod;

        public void SetSelectedGameMode(EGameMod gameMod)
        {
            _gameMod = gameMod;
        }
    }
}