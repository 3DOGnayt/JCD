using KoboldUi.Element.Controller;
using UnityEngine;
using UI.Canvas.MenuSelection.Views;
using Zenject;

namespace UI.Canvas.MenuSelection.Controllers
{
    public class OpponentSelectionController : AUiController<OpponentSelectionView>
    {
        [Inject] private MenuSelectionWindow _window;

        private bool _isReady;

        public override void Initialize()
        {
            if (_window == null || View == null || !View.IsValid)
            {
                Debug.LogError("[MenuSelection] OpponentSelectionController is missing required references.");
                return;
            }

            _isReady = true;

            View.ConfirmButton.onClick.AddListener(HandleConfirmOpponent);
            View.BackButton.onClick.AddListener(HandleBackToMap);

            _window.Flow.OpponentPanelShown += HandleOpponentPanelShown;
            _window.Flow.OpponentPanelReturnedFromRace += HandleOpponentReturnedFromRace;
        }

        private void HandleOpponentPanelShown()
        {
            View.SetOpponentThingsVisible(true);
        }

        private void HandleOpponentReturnedFromRace()
        {
            View.ShowOpponentThingsDelayed();
        }

        private void HandleConfirmOpponent()
        {
            if (!_isReady)
                return;

            View.SetOpponentThingsVisible(false);
            _window.Flow.ShowRacePanelFromOpponent();
        }

        private void HandleBackToMap()
        {
            if (!_isReady)
                return;

            _window.Flow.ShowMapPanelFromOpponent();
        }
    }
}
