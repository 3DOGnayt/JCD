using KoboldUi.Windows;
using UI.Controllers;
using UI.Views;
using UnityEngine;

namespace UI.Window
{
    public class TrainingOpponentWindow : AWindow
    {
        [SerializeField] private TrainingOpponentView _trainingOpponentView;
        protected override void AddControllers()
        {
            AddController<TrainingOpponentController, TrainingOpponentView>(_trainingOpponentView);
        }
    }
}