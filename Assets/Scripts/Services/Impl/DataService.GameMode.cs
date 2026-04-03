using Data.Enums;
using UnityEngine;

namespace Services.Impl
{
    public partial class DataService
    {
        private const string GameModeKey = "Selection.GameMode";

        public EGameMod LoadGameMode(EGameMod fallback)
        {
            var value = PlayerPrefs.GetInt(GameModeKey, (int)fallback);
            if (!System.Enum.IsDefined(typeof(EGameMod), value))
                return fallback;

            return (EGameMod)value;
        }

        public void SaveGameMode(EGameMod gameMode)
        {
            PlayerPrefs.SetInt(GameModeKey, (int)gameMode);
            PlayerPrefs.Save();
        }

        public void DeleteGameMode()
        {
            PlayerPrefs.DeleteKey(GameModeKey);
            PlayerPrefs.Save();
        }
    }
}