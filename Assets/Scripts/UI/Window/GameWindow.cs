using KoboldUi.Windows;
using UI.Controllers;
using UI.Views;
using UnityEngine;

namespace UI.Window
{
    public class GameWindow : AWindow
    {
        [SerializeField] private GameView _gameView;
        [Space]
        [SerializeField] private GameMapView _gameMapView;
        [SerializeField] private GameSpeedometerView _gameSpeedometerView;
        [SerializeField] private GameTimerView _gameTimerView;
        [SerializeField] private GameTrainingView _gameTrainingView;
        [SerializeField] private GameStoryView _gameStoryView;
        
        protected override void AddControllers()
        {
            AddController<GameController, GameView>(_gameView);
            
            AddController<GameMapController, GameMapView>(_gameMapView);
            AddController<GameSpeedometerController, GameSpeedometerView>(_gameSpeedometerView);
            AddController<GameTimerController, GameTimerView>(_gameTimerView);
            AddController<GameTrainingController, GameTrainingView>(_gameTrainingView);
            AddController<GameStoryController, GameStoryView>(_gameStoryView);
        }
    }
}