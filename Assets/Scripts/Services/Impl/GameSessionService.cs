using System.Collections.Generic;
using Configs.Impl;
using Data.Enums;
using KoboldUi.Services.WindowsService;
using Scellecs.Morpeh;
using Services;
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
        private readonly GameSelectionParameters _selectionParameters;
        private readonly World _world;

        private readonly List<GameObject> _runtimeInstances = new();
        private readonly List<Entity> _runtimeEntities = new();

        private bool _hasSnapshot;
        private Snapshot _snapshot;

        public GameSessionTarget Target { get; private set; } = GameSessionTarget.Menu;

        public GameSessionService(
            ILocalWindowsService localWindowsService,
            IRaceTimerService raceTimerService,
            ILoadingService loadingService,
            ISkidSmokeService skidSmokeService,
            ICrashEffectService crashEffectService,
            GameSelectionParameters selectionParameters,
            World world)
        {
            _localWindowsService = localWindowsService;
            _raceTimerService = raceTimerService;
            _loadingService = loadingService;
            _skidSmokeService = skidSmokeService;
            _crashEffectService = crashEffectService;
            _selectionParameters = selectionParameters;
            _world = world;
        }

        public void BeginGame()
        {
            CaptureSnapshot();
            Target = GameSessionTarget.Game;
            CleanupRuntime();
            OpenLoading();
        }

        public void RestartGame()
        {
            RestoreSnapshot();
            Target = GameSessionTarget.Game;
            CleanupRuntime();
            OpenLoading();
        }

        public void ExitToMenu()
        {
            Target = GameSessionTarget.Menu;
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
            _localWindowsService?.CloseToWindow<LoadingWindow>();
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
        }

        private void CaptureSnapshot()
        {
            if (_selectionParameters == null)
                return;

            _snapshot = new Snapshot
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
                SelectedOpponentIndex = _selectionParameters.SelectedOpponentIndex
            };

            _hasSnapshot = true;
        }

        private void RestoreSnapshot()
        {
            if (!_hasSnapshot || _selectionParameters == null)
                return;

            _selectionParameters.SetSelectedCar(
                _snapshot.SelectedCar,
                _snapshot.SelectedCarParameters,
                _snapshot.SelectedCarIndex);

            _selectionParameters.SetSelectedMap(
                _snapshot.SelectedMapPrefab,
                _snapshot.SelectedMapIndex,
                _snapshot.SelectedMap,
                _snapshot.SelectedMapSelectionCount,
                _snapshot.SelectedMapLapCount);

            _selectionParameters.SetSelectedGameMode(_snapshot.GameMod);
            _selectionParameters.SetSelectedOpponent(
                _snapshot.SelectedOpponentName,
                _snapshot.SelectedOpponentDifficulty,
                _snapshot.SelectedOpponentIndex);
        }

        private struct Snapshot
        {
            public CarPresetParameters SelectedCar;
            public CarParameters SelectedCarParameters;
            public int SelectedCarIndex;

            public GameObject SelectedMapPrefab;
            public int SelectedMapIndex;
            public int SelectedMapSelectionCount;
            public int SelectedMapLapCount;
            public EMap SelectedMap;

            public EGameMod GameMod;

            public string SelectedOpponentName;
            public float SelectedOpponentDifficulty;
            public int SelectedOpponentIndex;
        }
    }
}