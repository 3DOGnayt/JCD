using Configs.Impl;
using Data.Enums;
using Data.Struct;
using Zenject;

namespace Services.Impl
{
    public class SelectionPersistenceService : IInitializable
    {
        private readonly IDataService _dataService;
        private readonly IEventService _eventService;
        private readonly CarSelectionParameters _carSelectionParameters;
        private readonly MapSelectionParameters _mapSelectionParameters;
        private readonly GameModeSelectionParameters _gameModeSelectionParameters;
        private readonly OpponentSelectionParameters _opponentSelectionParameters;
        private readonly AudioSelectionParameters _audioSelectionParameters;
        private readonly CarCatalogParameters _carCatalogParameters;
        private readonly MapCatalogParameters _mapCatalogParameters;
        private readonly OpponentCatalogParameters _opponentCatalogParameters;
        private readonly TrainingTimeScoreParameters _trainingTimeScoreParameters;

        public SelectionPersistenceService(
            IDataService dataService,
            IEventService eventService,
            CarSelectionParameters carSelectionParameters,
            MapSelectionParameters mapSelectionParameters,
            GameModeSelectionParameters gameModeSelectionParameters,
            OpponentSelectionParameters opponentSelectionParameters,
            AudioSelectionParameters audioSelectionParameters,
            CarCatalogParameters carCatalogParameters,
            MapCatalogParameters mapCatalogParameters,
            OpponentCatalogParameters opponentCatalogParameters,
            TrainingTimeScoreParameters trainingTimeScoreParameters)
        {
            _dataService = dataService;
            _eventService = eventService;
            _carSelectionParameters = carSelectionParameters;
            _mapSelectionParameters = mapSelectionParameters;
            _gameModeSelectionParameters = gameModeSelectionParameters;
            _opponentSelectionParameters = opponentSelectionParameters;
            _audioSelectionParameters = audioSelectionParameters;
            _carCatalogParameters = carCatalogParameters;
            _mapCatalogParameters = mapCatalogParameters;
            _opponentCatalogParameters = opponentCatalogParameters;
            _trainingTimeScoreParameters = trainingTimeScoreParameters;
        }

        public void Initialize()
        {
            ApplySavedSelection();
            ApplyTrainingScores();
        }

        private void ApplySavedSelection()
        {
            var saved = _dataService.LoadGameSelection();
            if (saved == null || !saved.HasData)
            {
                EnsureDefaultCarSelection();
                ApplyGameModePreference(null);
                return;
            }

            ApplyCarSelection(saved);
            ApplyMapSelection(saved);
            ApplyOpponentSelection(saved);
            ApplyGameModeSelection(saved);
            ApplyMusicSelection(saved);
            ApplyGameModePreference(saved);
        }

        private void ApplyCarSelection(GameSelectionSaveData saved)
        {
            if (_carSelectionParameters == null || _carCatalogParameters == null)
                return;

            if (TryApplyCarSelection(saved.CarIndex))
                return;

            EnsureDefaultCarSelection();
        }

        private void EnsureDefaultCarSelection()
        {
            if (_carSelectionParameters == null)
                return;

            if (_carSelectionParameters.SelectedCar != null)
                return;

            _ = TryApplyCarSelection(0);
        }

        private bool TryApplyCarSelection(int index)
        {
            if (_carSelectionParameters == null || _carCatalogParameters == null)
                return false;

            var cars = _carCatalogParameters.Cars;
            if (index < 0 || index >= cars.Count)
                return false;

            var entry = cars[index];
            if (entry.Preset == null ||
                entry.MovementParameters == null ||
                entry.SpeedsPresetParameters == null ||
                entry.SlipParameters == null)
                return false;

            _carSelectionParameters.SetSelectedCar(
                entry.Preset,
                entry.MovementParameters,
                entry.SpeedsPresetParameters,
                entry.SlipParameters,
                entry.EngineAudioParameters,
                index);

            _eventService?.PublishCarSelectionChanged();
            return true;
        }

        private void ApplyMapSelection(GameSelectionSaveData saved)
        {
            if (_mapSelectionParameters == null || _mapCatalogParameters == null)
                return;

            var maps = _mapCatalogParameters.Maps;
            var mapIndex = saved.MapIndex;
            if (mapIndex < 0 || mapIndex >= maps.Count)
            {
                if (saved.Map == EMap.None)
                    return;

                mapIndex = FindMapIndexById(saved.Map);
                if (mapIndex < 0 || mapIndex >= maps.Count)
                    return;
            }

            var entry = maps[mapIndex];
            if (entry.Prefab == null)
                return;

            _mapSelectionParameters.SetSelectedMap(
                entry.Prefab,
                mapIndex,
                entry.EMap,
                entry.SelectionCount,
                entry.LapCount);
        }

        private int FindMapIndexById(EMap map)
        {
            var maps = _mapCatalogParameters.Maps;
            for (var i = 0; i < maps.Count; i++)
            {
                if (maps[i].EMap == map)
                    return i;
            }

            return -1;
        }

        private void ApplyOpponentSelection(GameSelectionSaveData saved)
        {
            if (_opponentSelectionParameters == null || _opponentCatalogParameters == null)
                return;

            var opponents = _opponentCatalogParameters.Opponents;
            if (saved.OpponentIndex < 0 || saved.OpponentIndex >= opponents.Count)
                return;

            var entry = opponents[saved.OpponentIndex];
            _opponentSelectionParameters.SetSelectedOpponent(entry.Base.DisplayName, entry.Base.Difficulty, saved.OpponentIndex);
        }

        private void ApplyGameModeSelection(GameSelectionSaveData saved)
        {
            if (_gameModeSelectionParameters == null || saved.GameMode == EGameMod.None)
                return;

            _gameModeSelectionParameters.SetSelectedGameMode(saved.GameMode);
        }

        private void ApplyGameModePreference(GameSelectionSaveData saved)
        {
            if (_gameModeSelectionParameters == null || _dataService == null)
                return;

            if (saved != null && saved.GameMode != EGameMod.None)
                return;

            var preferred = _dataService.LoadGameMode(EGameMod.Training);
            if (preferred == EGameMod.None)
                preferred = EGameMod.Training;

            _gameModeSelectionParameters.SetSelectedGameMode(preferred);
        }

        private void ApplyMusicSelection(GameSelectionSaveData saved)
        {
            if (_audioSelectionParameters == null || saved.MusicSubType == EAudioSubType.None)
                return;

            var index = saved.MusicIndex < 0 ? 0 : saved.MusicIndex;
            _audioSelectionParameters.SetSelectedMusic(saved.MusicSubType, index);
        }

        private void ApplyTrainingScores()
        {
            if (_trainingTimeScoreParameters == null)
                return;

            var entries = _dataService.LoadTrainingTimeScores();
            if (entries == null || entries.Count == 0)
                return;

            _trainingTimeScoreParameters.ReplaceEntries(entries);
        }
    }
}