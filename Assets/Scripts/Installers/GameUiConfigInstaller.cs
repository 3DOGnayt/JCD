using KoboldUi.Utils;
using UI.Window;
using UnityEngine;
using Zenject;

namespace Installers
{
    [CreateAssetMenu(menuName = "Game/UI/" + nameof(GameUiConfigInstaller), fileName = nameof(GameUiConfigInstaller), order = 0)]
    public class GameUiConfigInstaller : ScriptableObjectInstaller
    {
        [Header("Game Windows")]
        [SerializeField] private GameWindow _gameWindow;
        [SerializeField] private GamePauseWindow _gamePauseWindow;
        [SerializeField] private GameResultWindow _gameResultWindow;

        public override void InstallBindings()
        {
            var canvas = Container.Resolve<Canvas>();
            
            Container.BindWindowFromPrefab(canvas, _gameWindow);
            Container.BindWindowFromPrefab(canvas, _gamePauseWindow);
            Container.BindWindowFromPrefab(canvas, _gameResultWindow);
        }
    }
}