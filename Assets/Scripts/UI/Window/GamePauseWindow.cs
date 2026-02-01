using KoboldUi.Windows;
using UI.Controllers;
using UI.Views;
using UnityEngine;

namespace UI.Window
{
    public class GamePauseWindow : AWindow
    {
        [SerializeField] private GamePauseView _gamePauseView;
        protected override void AddControllers()
        {
            AddController<GamePauseController, GamePauseView>(_gamePauseView);
        }
    }
}