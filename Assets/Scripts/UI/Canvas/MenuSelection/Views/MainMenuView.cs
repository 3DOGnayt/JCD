using KoboldUi.Element.View;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Canvas.MenuSelection.Views
{
    public class MainMenuView : AUiView
    {
        [SerializeField] private RectTransform _panel;
        [SerializeField] private Button _openCarPanelButton;
        [SerializeField] private Button _openSettingsButton;
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _exitButton;

        public RectTransform PanelTransform => _panel;
        public Button OpenCarPanelButton => _openCarPanelButton;
        public Button OpenSettingsButton => _openSettingsButton;
        public Button StartButton => _startButton;
        public Button ExitButton => _exitButton;

        public bool IsValid => _panel != null
                              && _openCarPanelButton != null
                              && _openSettingsButton != null
                              && _startButton != null
                              && _exitButton != null;

        public void SetPanelActive(bool isActive)
        {
            _panel.gameObject.SetActive(isActive);
        }

        public void SetButtonsInteractable(bool isInteractable)
        {
            _openCarPanelButton.interactable = isInteractable;
            _openSettingsButton.interactable = isInteractable;
            _startButton.interactable = isInteractable;
            _exitButton.interactable = isInteractable;
        }

        public override void CloseInstantly()
        {
            _panel.gameObject.SetActive(false);
        }
    }
}
