using KoboldUi.Windows;
using UI.Controllers;
using UI.Views;
using UnityEngine;

namespace UI.Window
{
    public class GameResultWindow : AWindow
    {
        [SerializeField] private GameResultView _gameResultView;
        protected override void AddControllers()
        {
            AddController<GameResultController, GameResultView>(_gameResultView);
        }
    }
}