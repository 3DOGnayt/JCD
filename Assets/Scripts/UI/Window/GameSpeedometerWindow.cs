using KoboldUi.Windows;
using UI.Controllers;
using UI.Views;
using UnityEngine;

namespace UI.Window
{
    public class GameSpeedometerWindow : AWindow
    {
        [SerializeField] private GameSpeedometerView _gameSpeedometerView;
        protected override void AddControllers()
        {
            AddController<GameSpeedometerController, GameSpeedometerView>(_gameSpeedometerView);
        }
    }
}