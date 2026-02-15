using KoboldUi.Windows;
using UI.Controllers;
using UI.Views;
using UnityEngine;

namespace UI.Window
{
    public class GarageWindow : AWindow
    {
        [SerializeField] private GarageView _garageView;
        protected override void AddControllers()
        {
            AddController<GarageController, GarageView>(_garageView);
        }
    }
}