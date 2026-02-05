using KoboldUi.Windows;
using UI.Controllers;
using UI.Views;
using UnityEngine;

namespace UI.Window
{
    public class GameWindow : AWindow
    {
        [SerializeField] private GameMapView _gameMapView;
        [SerializeField] private GameSpeedometerView _gameSpeedometerView;
        [SerializeField] private GameTimerView _gameTimerView;
        [SerializeField] private TrainingOpponentView _trainingOpponentView;
        protected override void AddControllers()
        {
            AddController<GameMapController, GameMapView>(_gameMapView);
            AddController<GameSpeedometerController, GameSpeedometerView>(_gameSpeedometerView);
            AddController<GameTimerController, GameTimerView>(_gameTimerView);
            AddController<TrainingOpponentController, TrainingOpponentView>(_trainingOpponentView);
        }
    }
}