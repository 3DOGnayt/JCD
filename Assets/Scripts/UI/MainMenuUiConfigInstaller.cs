using KoboldUi.Utils;
using UI.Window;
using UnityEngine;
using Zenject;

namespace UI
{
    [CreateAssetMenu(menuName = "Game/UI/" + nameof(MainMenuUiConfigInstaller), fileName = nameof(MainMenuUiConfigInstaller), order = 0)]
    public class MainMenuUiConfigInstaller : ScriptableObjectInstaller
    {
        
        [Header("MainMenu Windows")]
        [SerializeField] private MainMenuWindow _mainMenuWindow;
        [SerializeField] private SettingsWindow _settingsWindow;
        [SerializeField] private GarageWindow _garageWindow;
        [SerializeField] private MapWindow _mapWindow;
        [SerializeField] private OpponentWindow _opponentWindow;
        [SerializeField] private GameModWindow _gameModWindow;
        [SerializeField] private LoadingWindow _loadingWindow;

        public override void InstallBindings()
        {
            var canvas = Container.Resolve<Canvas>();
            
            Container.BindWindowFromPrefab(canvas, _mainMenuWindow);
            Container.BindWindowFromPrefab(canvas, _settingsWindow);
            Container.BindWindowFromPrefab(canvas, _garageWindow);
            Container.BindWindowFromPrefab(canvas, _mapWindow);
            Container.BindWindowFromPrefab(canvas, _opponentWindow);
            Container.BindWindowFromPrefab(canvas, _gameModWindow);
            Container.BindWindowFromPrefab(canvas, _loadingWindow);
        }
    }
}