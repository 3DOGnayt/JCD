using KoboldUi.Element.View;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Canvas.MenuSelection.Views
{
    public class RaceSelectionView : AUiView
    {
        [SerializeField] private RectTransform _panel;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _backButton;

        public RectTransform PanelTransform => _panel;
        public Button ConfirmButton => _confirmButton;
        public Button BackButton => _backButton;

        public bool IsValid => _panel != null
                              && _confirmButton != null
                              && _backButton != null;

        public void SetPanelActive(bool isActive)
        {
            _panel.gameObject.SetActive(isActive);
        }

        public override void CloseInstantly()
        {
            _panel.gameObject.SetActive(false);
        }
    }
}
