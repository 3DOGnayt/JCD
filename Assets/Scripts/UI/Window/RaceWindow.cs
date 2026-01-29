using KoboldUi.Windows;
using UI.Controllers;
using UI.Views;
using UnityEngine;

namespace UI.Window
{
    public class RaceWindow : AWindow
    {
        [SerializeField] private RaceView _raceView;
        protected override void AddControllers()
        {
            AddController<RaceController, RaceView>(_raceView);
        }
    }
}