using KoboldUi.Windows;
using UI.Canvas.Controllers;
using UI.Canvas.Views;
using UnityEngine;

namespace UI.Canvas.Window
{
    public class MainMenuWindow : AWindow
    {
        [SerializeField] private MainMenuView mainMenuView;
        //[SerializeField] private TitleView titleView;
        
        protected override void AddControllers()
        {
            AddController<MainMenuController, MainMenuView>(mainMenuView);
            //AddController<TitleController, TitleView>(titleView);
        }
    }
}