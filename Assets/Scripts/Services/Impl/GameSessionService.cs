using System.Collections.Generic;
using Configs.Impl;
using Data.Enums;
using Data.Struct;
using KoboldUi.Services.WindowsService;
using Scellecs.Morpeh;
using UI.Window;
using UnityEngine;

namespace Services.Impl
{
    public class GameSessionService : IGameSessionService
    {
        private readonly ILocalWindowsService _localWindowsService;
        private readonly IRaceTimerService _raceTimerService;
        private readonly ILoadingService _loadingService;
        private readonly ISkidSmokeService _skidSmokeService;
        private readonly ICrashEffectService _crashEffectService;
        private readonly ISkidmarksService _skidmarksService;
        private readonly GameSelectionParameters _selectionParameters;
        private readonly World _world;

        private readonly List<GameObject> _runtimeInstances = new();
        private readonly List<Entity> _runtimeEntities = new();

        private bool _hasSnapshot;
        private GameSessionSnapshot _gameSessionSnapshot;

        public EGameSessionTarget Target { get; private set; } = EGameSessionTarget.Menu;

        public GameSessionService(
            ILocalWindowsService localWindowsService,
            IRaceTimerService raceTimerService,
            ILoadingService loadingService,
            ISkidSmokeService skidSmokeService,
            ICrashEffectService crashEffectService,
            ISkidmarksService skidmarksService,
            GameSelectionParameters selectionParameters,
            World world)
        {
            _localWindowsService = localWindowsService;
            _raceTimerService = raceTimerService;
            _loadingService = loadingService;
            _skidSmokeService = skidSmokeService;
            _crashEffectService = crashEffectService;
            _skidmarksService = skidmarksService;
            _selectionParameters = selectionParameters;
            _world = world;
        }

        public void BeginGame()
        {
            CaptureSnapshot();
            Target = EGameSessionTarget.Game;
            CleanupRuntime();
            OpenLoading();
        }

        public void RestartGame()
        {
            RestoreSnapshot();
            Target = EGameSessionTarget.Game;
            CleanupRuntime();
            OpenLoading();
        }

        public void ExitToMenu()
        {
            Target = EGameSessionTarget.Menu;
            CleanupRuntime();
            OpenLoading();
        }

        public void RegisterRuntimeRoot(GameObject instance)
        {
            if (instance == null || _runtimeInstances.Contains(instance))
                return;

            _runtimeInstances.Add(instance);
        }

        public void RegisterRuntimeEntity(Entity entity)
        {
            if (_world != null && _world.Has(entity))
                _runtimeEntities.Add(entity);
        }

        private void OpenLoading()
        {
            _loadingService?.PublishInputEnabled(false);
            _localWindowsService?.CloseAllWindows();
            _localWindowsService?.OpenWindow<LoadingWindow>();
        }

        private void CleanupRuntime()
        {
            if (_world != null)
            {
                for (var i = 0; i < _runtimeEntities.Count; i++)
                {
                    var entity = _runtimeEntities[i];
                    if (_world.Has(entity))
                        _world.RemoveEntity(entity);
                }
            }

            _runtimeEntities.Clear();

            for (var i = 0; i < _runtimeInstances.Count; i++)
            {
                var instance = _runtimeInstances[i];
                
                Object.Destroy(instance);
            }

            _runtimeInstances.Clear();

            _loadingService?.ResetEvents();
            _raceTimerService?.ResetRace();
            _skidSmokeService?.ResetPool();
            _crashEffectService?.ResetPool();
            _skidmarksService?.ResetMesh();
        }

        private void CaptureSnapshot()
        {
            if (_selectionParameters == null)
                return;

            _gameSessionSnapshot = new GameSessionSnapshot
            {
                SelectedCar = _selectionParameters.SelectedCar,
                SelectedCarParameters = _selectionParameters.SelectedCarParameters,
                SelectedCarIndex = _selectionParameters.SelectedCarIndex,
                SelectedMapPrefab = _selectionParameters.SelectedMapPrefab,
                SelectedMapIndex = _selectionParameters.SelectedMapIndex,
                SelectedMapSelectionCount = _selectionParameters.SelectedMapSelectionCount,
                SelectedMapLapCount = _selectionParameters.SelectedMapLapCount,
                SelectedMap = _selectionParameters.SelectedMap,
                GameMod = _selectionParameters.GameMod,
                SelectedOpponentName = _selectionParameters.SelectedOpponentName,
                SelectedOpponentDifficulty = _selectionParameters.SelectedOpponentDifficulty,
                SelectedOpponentIndex = _selectionParameters.SelectedOpponentIndex,
                SelectedMusicSubType = _selectionParameters.SelectedMusicSubType,
                SelectedMusicIndex = _selectionParameters.SelectedMusicIndex
            };

            _hasSnapshot = true;
        }

        private void RestoreSnapshot()
        {
            if (!_hasSnapshot || _selectionParameters == null)
                return;

            _selectionParameters.SetSelectedCar(
                _gameSessionSnapshot.SelectedCar,
                _gameSessionSnapshot.SelectedCarParameters,
                _gameSessionSnapshot.SelectedCarIndex);

            _selectionParameters.SetSelectedMap(
                _gameSessionSnapshot.SelectedMapPrefab,
                _gameSessionSnapshot.SelectedMapIndex,
                _gameSessionSnapshot.SelectedMap,
                _gameSessionSnapshot.SelectedMapSelectionCount,
                _gameSessionSnapshot.SelectedMapLapCount);

            _selectionParameters.SetSelectedGameMode(_gameSessionSnapshot.GameMod);
            
            _selectionParameters.SetSelectedOpponent(
                _gameSessionSnapshot.SelectedOpponentName,
                _gameSessionSnapshot.SelectedOpponentDifficulty,
                _gameSessionSnapshot.SelectedOpponentIndex);

            _selectionParameters.SetSelectedMusic(
                _gameSessionSnapshot.SelectedMusicSubType,
                _gameSessionSnapshot.SelectedMusicIndex);
        }
    }
}