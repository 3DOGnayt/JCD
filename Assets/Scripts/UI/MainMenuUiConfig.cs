using KoboldUi.Utils;
using UI.Window;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace UI
{
    [CreateAssetMenu(fileName = nameof(MainMenuUiConfig), menuName = "UI/" + nameof(MainMenuUiConfig), order = 0)]
    public class MainMenuUiConfig : ScriptableObjectInstaller
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private EventSystem _eventSystem;
        
        [Header("Windows")]
        [SerializeField] private MainMenuWindow _mainMenuWindow;
        [SerializeField] private SettingsWindow _settingsWindow;
        [SerializeField] private GarageWindow _garageWindow;
        [SerializeField] private MapWindow _mapWindow;

        public override void InstallBindings()
        {
            var canvasInstance = Instantiate(_canvas);
            Container.Bind<Canvas>().FromInstance(canvasInstance).AsSingle();
            Container.Bind<EventSystem>().FromInstance(_eventSystem).AsSingle();
            
            Container.BindWindowFromPrefab(canvasInstance, _mainMenuWindow);
            Container.BindWindowFromPrefab(canvasInstance, _settingsWindow);
            Container.BindWindowFromPrefab(canvasInstance, _garageWindow);
            Container.BindWindowFromPrefab(canvasInstance, _mapWindow);
        }
    }
}