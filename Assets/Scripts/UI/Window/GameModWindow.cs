using KoboldUi.Windows;
using UI.Controllers;
using UI.Views;
using UnityEngine;

namespace UI.Window
{
    public class GameModWindow : AWindow
    {
        [SerializeField] private GameModView _gameModView;
        protected override void AddControllers()
        {
            AddController<GameModController, GameModView>(_gameModView);
        }
    }
}