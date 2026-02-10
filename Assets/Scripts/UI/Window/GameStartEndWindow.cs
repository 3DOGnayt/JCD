using KoboldUi.Windows;
using UI.Controllers;
using UI.Views;
using UnityEngine;

namespace UI.Window
{
    public class GameStartEndWindow : AWindow
    {
        [SerializeField] private GameStartEndView _gameStartEndView;
        protected override void AddControllers()
        {
            AddController<GameStartEndController, GameStartEndView>(_gameStartEndView);
        }
    }
}