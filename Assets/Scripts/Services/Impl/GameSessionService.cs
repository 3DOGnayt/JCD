using System.Collections.Generic;
using Configs.Impl;
using Data.Enums;
using Data.HelperClass;
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
        private readonly IEventService _eventService;
        private readonly ISkidSmokeService _skidSmokeService;
        private readonly ICrashEffectService _crashEffectService;
        private readonly ISkidmarksService _skidmarksService;
        private readonly IDataService _dataService;
        private readonly GameSelectionParameters _selectionParameters;
        private readonly World _world;

        private readonly List<GameObject> _runtimeInstances = new();
        private readonly List<Entity> _runtimeEntities = new();

        public EGameSessionTarget Target { get; private set; } = EGameSessionTarget.Menu;

        public GameSessionService(
            ILocalWindowsService localWindowsService,
            IRaceTimerService raceTimerService,
            IEventService eventService,
            ISkidSmokeService skidSmokeService,
            ICrashEffectService crashEffectService,
            ISkidmarksService skidmarksService,
            IDataService dataService,
            GameSelectionParameters selectionParameters,
            World world)
        {
            _localWindowsService = localWindowsService;
            _raceTimerService = raceTimerService;
            _eventService = eventService;
            _skidSmokeService = skidSmokeService;
            _crashEffectService = crashEffectService;
            _skidmarksService = skidmarksService;
            _dataService = dataService;
            _selectionParameters = selectionParameters;
            _world = world;
        }

        public void BeginGame()
        {
            SaveSelection();
            Target = EGameSessionTarget.Game;
            CleanupRuntime();
            OpenLoading();
        }

        public void RestartGame()
        {
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
            _eventService?.PublishInputEnabled(false);
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
            _selectionParameters?.MapSelectionParameters?.SetRuntimeSpline(null);

            _eventService?.ResetEvents();
            _raceTimerService?.ResetRace();
            _skidSmokeService?.ResetPool();
            _crashEffectService?.ResetPool();
            _skidmarksService?.ResetMesh();
        }

        private void SaveSelection()
        {
            if (_dataService == null || _selectionParameters == null)
                return;

            var data = new GameSelectionSaveData
            {
                CarIndex = _selectionParameters.SelectedCarIndex,
                MapIndex = _selectionParameters.SelectedMapIndex,
                Map = _selectionParameters.SelectedMap,
                GameMode = _selectionParameters.GameMod,
                OpponentIndex = _selectionParameters.SelectedOpponentIndex,
                MusicSubType = _selectionParameters.SelectedMusicSubType,
                MusicIndex = _selectionParameters.SelectedMusicIndex
            };

            _dataService.SaveGameSelection(data);
        }
    }
}