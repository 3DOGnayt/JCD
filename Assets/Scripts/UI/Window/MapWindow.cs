using KoboldUi.Windows;
using UI.Controllers;
using UI.Views;
using UnityEngine;

namespace UI.Window
{
    public class MapWindow : AWindow
    {
        [SerializeField] private MapView _mapView;
        
        protected override void AddControllers()
        {
            AddController<MapController, MapView>(_mapView);
            AddController<AudioMenuController, MapView>(_mapView);
        }
    }
}