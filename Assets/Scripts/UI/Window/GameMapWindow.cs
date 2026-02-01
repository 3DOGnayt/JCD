using KoboldUi.Windows;
using UI.Controllers;
using UI.Views;
using UnityEngine;

namespace UI.Window
{
    public class GameMapWindow : AWindow
    {
        [SerializeField] private GameMapView _gameMapView;
        protected override void AddControllers()
        {
            AddController<GameMapController, GameMapView>(_gameMapView);
        }
    }
}