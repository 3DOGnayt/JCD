using KoboldUi.Windows;
using UI.Controllers;
using UI.Views;
using UnityEngine;

namespace UI.Window
{
    public class OpponentWindow : AWindow
    {
        [SerializeField] private OpponentView _opponentView;
        protected override void AddControllers()
        {
            AddController<OpponentController, OpponentView>(_opponentView);
        }
    }
}