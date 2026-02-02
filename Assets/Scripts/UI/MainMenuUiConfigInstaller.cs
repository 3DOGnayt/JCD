using KoboldUi.Utils;
using UI.Window;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace UI
{
    [CreateAssetMenu(menuName = "Game/UI/" + nameof(MainMenuUiConfigInstaller), fileName = nameof(MainMenuUiConfigInstaller), order = 0)]
    public class MainMenuUiConfigInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private EventSystem _eventSystem;
        
        [Header("Windows")]
        [SerializeField] private MainMenuWindow _mainMenuWindow;
        [SerializeField] private SettingsWindow _settingsWindow;
        [SerializeField] private GarageWindow _garageWindow;
        [SerializeField] private MapWindow _mapWindow;
        [SerializeField] private OpponentWindow _opponentWindow;
        [SerializeField] private RaceWindow _raceWindow;

        public override void InstallBindings()
        {
            var canvasInstance = Instantiate(_canvas);
            var eventSystemInstance = Instantiate(_eventSystem);
            
            Container.Bind<Canvas>().FromInstance(canvasInstance).AsSingle();
            Container.Bind<EventSystem>().FromInstance(eventSystemInstance).AsSingle();
            
            Container.BindWindowFromPrefab(canvasInstance, _mainMenuWindow);
            Container.BindWindowFromPrefab(canvasInstance, _settingsWindow);
            Container.BindWindowFromPrefab(canvasInstance, _garageWindow);
            Container.BindWindowFromPrefab(canvasInstance, _mapWindow);
            Container.BindWindowFromPrefab(canvasInstance, _opponentWindow);
            Container.BindWindowFromPrefab(canvasInstance, _raceWindow);
        }
    }
}