using KoboldUi.Element.Controller;
using UnityEngine;
using UI.Canvas.MenuSelection.Views;
using Zenject;

namespace UI.Canvas.MenuSelection.Controllers
{
    public class MainMenuController : AUiController<MainMenuView>
    {
        [Inject] private MenuSelectionWindow _window;

        private bool _isReady;

        public override void Initialize()
        {
            if (_window == null || View == null || !View.IsValid)
            {
                Debug.LogError("[MenuSelection] MainMenuController is missing required references.");
                return;
            }

            _isReady = true;

            View.OpenCarPanelButton.onClick.AddListener(HandleOpenCarPanel);
            View.OpenSettingsButton.onClick.AddListener(HandleOpenSettings);
            View.StartButton.onClick.AddListener(HandleStart);
            View.ExitButton.onClick.AddListener(Application.Quit);
        }

        private void HandleOpenCarPanel()
        {
            if (!_isReady)
                return;

            _window.Flow.ShowCarPanel();
        }

        private void HandleOpenSettings()
        {
            if (!_isReady)
                return;

            _window.Flow.ShowSettingsPanel();
        }

        private void HandleStart()
        {
            if (!_isReady)
                return;

            _window.Flow.ShowMapPanel();
        }

        protected override void OnOpen()
        {
            if (!_isReady)
                return;

            _window.Flow.ShowMainPanel();
        }
    }
}
