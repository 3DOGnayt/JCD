using System.Collections.Generic;
using KoboldUi.Element.View;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Canvas.MenuSelection.Views
{
    public class CarSelectionView : AUiView
    {
        [SerializeField] private RectTransform _panel;
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Image _presentation;
        [SerializeField] private List<Button> _carButtons = new List<Button>();

        public RectTransform PanelTransform => _panel;
        public Button BackButton => _backButton;
        public Button ConfirmButton => _confirmButton;
        public Image Presentation => _presentation;
        public List<Button> CarButtons => _carButtons;

        public bool IsValid => _panel != null
                              && _backButton != null
                              && _confirmButton != null
                              && _presentation != null
                              && _carButtons != null;

        public void SetPanelActive(bool isActive)
        {
            _panel.gameObject.SetActive(isActive);
        }

        public void SetPresentation(Sprite sprite)
        {
            _presentation.sprite = sprite;
            _presentation.enabled = _presentation.sprite != null;
        }

        public override void CloseInstantly()
        {
            _panel.gameObject.SetActive(false);
        }
    }
}
