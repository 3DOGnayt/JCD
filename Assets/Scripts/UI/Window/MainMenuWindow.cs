using KoboldUi.Windows;
using UI.Controllers;
using UI.Views;
using UnityEngine;

namespace UI.Window
{
    public class MainMenuWindow : AWindow
    {
        [SerializeField] private MainMenuView mainMenuView;
        
        protected override void AddControllers()
        {
            AddController<MainMenuController, MainMenuView>(mainMenuView);
        }
    }
}