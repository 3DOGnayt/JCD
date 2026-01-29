using KoboldUi.Windows;
using UI.Controllers;
using UI.Views;
using UnityEngine;

namespace UI.Window
{
    public class SettingsWindow : AWindow
    {
        [SerializeField] private SettingsView _settingsView;
            
        protected override void AddControllers()
        {
            AddController<SettingsController, SettingsView>(_settingsView);
        }
    }
}