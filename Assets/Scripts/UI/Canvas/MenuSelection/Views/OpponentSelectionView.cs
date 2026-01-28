using System.Collections;
using KoboldUi.Element.View;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Canvas.MenuSelection.Views
{
    public class OpponentSelectionView : AUiView
    {
        [SerializeField] private RectTransform _panel;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _backButton;
        [SerializeField] private GameObject _opponentThings;
        [SerializeField] private float _opponentThingsDelay = 0.1f;

        private Coroutine _opponentThingsRoutine;

        public RectTransform PanelTransform => _panel;
        public Button ConfirmButton => _confirmButton;
        public Button BackButton => _backButton;
        public GameObject OpponentThings => _opponentThings;
        public float OpponentThingsDelay => _opponentThingsDelay;

        public bool IsValid => _panel != null
                              && _confirmButton != null
                              && _backButton != null
                              && _opponentThings != null;

        public void SetPanelActive(bool isActive)
        {
            _panel.gameObject.SetActive(isActive);
        }

        public void SetOpponentThingsVisible(bool isVisible)
        {
            _opponentThings.SetActive(isVisible);
            _confirmButton.gameObject.SetActive(isVisible);
            _backButton.gameObject.SetActive(isVisible);
        }

        public void ShowOpponentThingsDelayed()
        {
            if (_opponentThingsRoutine != null)
                StopCoroutine(_opponentThingsRoutine);

            _opponentThingsRoutine = StartCoroutine(ShowOpponentThingsAfterDelay());
        }

        public override void CloseInstantly()
        {
            _panel.gameObject.SetActive(false);
        }

        private IEnumerator ShowOpponentThingsAfterDelay()
        {
            var delay = Mathf.Max(0f, _opponentThingsDelay);
            if (delay > 0f)
                yield return new WaitForSeconds(delay);

            SetOpponentThingsVisible(true);
        }
    }
}
