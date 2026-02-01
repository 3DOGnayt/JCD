using KoboldUi.Windows;
using UI.Controllers;
using UI.Views;
using UnityEngine;

namespace UI.Window
{
    public class GameTimerWindow : AWindow
    {
        [SerializeField] private GameTimerView _gameTimerView;
        protected override void AddControllers()
        {
            AddController<GameTimerController, GameTimerView>(_gameTimerView);
        }
    }
}