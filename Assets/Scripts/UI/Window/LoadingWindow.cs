using KoboldUi.Windows;
using UI.Controllers;
using UI.Views;
using UnityEngine;

namespace UI.Window
{
    public class LoadingWindow : AWindow
    {
        [SerializeField] private LoadingView _loadingView;
        protected override void AddControllers()
        {
            AddController<LoadingController, LoadingView>(_loadingView);
        }
    }
}