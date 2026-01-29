using KoboldUi.Windows;
using UI.Controllers;
using UI.Views;
using UnityEngine;

namespace UI.Window
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