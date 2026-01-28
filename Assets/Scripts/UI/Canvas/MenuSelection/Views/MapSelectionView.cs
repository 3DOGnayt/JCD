using System.Collections;
using System.Collections.Generic;
using KoboldUi.Element.View;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Canvas.MenuSelection.Views
{
    public class MapSelectionView : AUiView
    {
        [SerializeField] private RectTransform _panel;
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Image _presentation;
        [SerializeField] private float _presentationDelay = 0.1f;
        [SerializeField] private List<Button> _mapButtons = new List<Button>();

        private Coroutine _presentationRoutine;
        private Coroutine _confirmRoutine;

        public RectTransform PanelTransform => _panel;
        public Button BackButton => _backButton;
        public Button ConfirmButton => _confirmButton;
        public Image Presentation => _presentation;
        public float PresentationDelay => _presentationDelay;
        public List<Button> MapButtons => _mapButtons;

        public bool IsValid => _panel != null
                              && _backButton != null
                              && _confirmButton != null
                              && _presentation != null
                              && _mapButtons != null;

        public void SetPanelActive(bool isActive)
        {
            _panel.gameObject.SetActive(isActive);
        }

        public void SetPresentation(Sprite sprite)
        {
            _presentation.sprite = sprite;
            _presentation.enabled = _presentation.sprite != null;
        }

        public void SetPresentationVisible(bool isVisible)
        {
            _presentation.gameObject.SetActive(isVisible);
        }

        public void SetConfirmBackVisible(bool isVisible)
        {
            _confirmButton.gameObject.SetActive(isVisible);
            _backButton.gameObject.SetActive(isVisible);
        }

        public void ShowPresentationDelayed()
        {
            if (_presentationRoutine != null)
                StopCoroutine(_presentationRoutine);

            _presentationRoutine = StartCoroutine(ShowPresentationAfterDelay());
        }

        public void ShowConfirmBackDelayed()
        {
            if (_confirmRoutine != null)
                StopCoroutine(_confirmRoutine);

            _confirmRoutine = StartCoroutine(ShowConfirmBackAfterDelay());
        }

        public override void CloseInstantly()
        {
            _panel.gameObject.SetActive(false);
        }

        private IEnumerator ShowPresentationAfterDelay()
        {
            var delay = Mathf.Max(0f, _presentationDelay);
            if (delay > 0f)
                yield return new WaitForSeconds(delay);

            _presentation.gameObject.SetActive(true);
        }

        private IEnumerator ShowConfirmBackAfterDelay()
        {
            var delay = Mathf.Max(0f, _presentationDelay);
            if (delay > 0f)
                yield return new WaitForSeconds(delay);

            _confirmButton.gameObject.SetActive(true);
            _backButton.gameObject.SetActive(true);
        }
    }
}
