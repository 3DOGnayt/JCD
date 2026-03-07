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
    }
}