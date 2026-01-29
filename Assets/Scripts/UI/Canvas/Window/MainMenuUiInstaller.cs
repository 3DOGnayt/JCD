using KoboldUi.Utils;
using UnityEngine;
using Zenject;

namespace UI.Canvas.Window
{
    [CreateAssetMenu(fileName = nameof(MainMenuUiInstaller), menuName = "UI/" + nameof(MainMenuUiInstaller), order = 0)]
    public class MainMenuUiInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private UnityEngine.Canvas canvas;
        
        [Header("Windows")]
        [SerializeField] private MainMenuWindow mainMenuWindow;
        //[SerializeField] private SettingsWindow settingsWindow;
        //[SerializeField] private SettingsChangeConfirmationWindow settingsChangeConfirmationWindow;
        //[SerializeField] private LevelSelectorWindow levelSelectorWindow;

        public override void InstallBindings()
        {
            var canvasInstance = Instantiate(canvas);
            
            Container.BindWindowFromPrefab(canvasInstance, mainMenuWindow);
            //Container.BindWindowFromPrefab(canvasInstance, settingsWindow);
            //Container.BindWindowFromPrefab(canvasInstance, settingsChangeConfirmationWindow);
            //Container.BindWindowFromPrefab(canvasInstance, levelSelectorWindow);
        }
    }
}