using KoboldUi.Element.Controller;
using KoboldUi.Services.WindowsService;
using Signals;
using UnityEngine;
using UI.Canvas.MenuSelection.Views;
using Zenject;

namespace UI.Canvas.MenuSelection.Controllers
{
    public class RaceSelectionController : AUiController<RaceSelectionView>
    {
        [Inject] private MenuSelectionWindow _window;
        [Inject] private SignalBus _signalBus;
        [Inject] private ILocalWindowsService _windowsService;

        private bool _isReady;

        public override void Initialize()
        {
            if (_window == null || View == null || !View.IsValid || _signalBus == null || _windowsService == null)
            {
                Debug.LogError("[MenuSelection] RaceSelectionController is missing required references.");
                return;
            }

            _isReady = true;

            View.ConfirmButton.onClick.AddListener(HandleConfirmRace);
            View.BackButton.onClick.AddListener(HandleBackFromRace);
        }

        private void HandleConfirmRace()
        {
            if (!_isReady)
                return;

            _signalBus.Fire(new StartRaceSignal());
            _windowsService.CloseWindow(null, false);
        }

        private void HandleBackFromRace()
        {
            if (!_isReady)
                return;

            _window.Flow.ShowOpponentPanelFromRace();
        }
    }
}
