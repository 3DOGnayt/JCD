using System.IO;
using Data.Enums;
using Data.Struct;

namespace Services.Impl
{
    public partial class DataService
    {
        private const string GameSelectionFileName = "game_selection.json";

        public GameSelectionSaveData LoadGameSelection()
        {
            var fallback = new GameSelectionSaveData();
            return LoadJson(GameSelectionFileName, fallback);
        }

        public void SaveGameSelection(GameSelectionSaveData data)
        {
            if (data == null)
                return;

            data.HasData = true;
            SaveJson(GameSelectionFileName, data);
        }

        public void DeleteGameSelection()
        {
            var path = GetJsonPath(GameSelectionFileName);
            if (File.Exists(path))
                File.Delete(path);
        }

        public void ClearSavedCarSelection()
        {
            UpdateSelectionData(data =>
            {
                data.CarIndex = -1;
            });
        }

        public void ClearSavedMapSelection()
        {
            UpdateSelectionData(data =>
            {
                data.MapIndex = -1;
                data.Map = EMap.None;
            });
        }

        public void ClearSavedGameModeSelection()
        {
            DeleteGameMode();
            UpdateSelectionData(data =>
            {
                data.GameMode = EGameMod.None;
            });
        }

        public void ClearSavedOpponentSelection()
        {
            UpdateSelectionData(data =>
            {
                data.OpponentIndex = -1;
            });
        }

        public void ClearSavedMusicSelection()
        {
            UpdateSelectionData(data =>
            {
                data.MusicSubType = EAudioSubType.None;
                data.MusicIndex = -1;
            });
        }

        private void UpdateSelectionData(System.Action<GameSelectionSaveData> updater)
        {
            if (updater == null)
                return;

            var data = LoadGameSelection();
            if (data == null || !data.HasData)
                return;

            updater(data);
            SaveGameSelection(data);
        }
    }
}