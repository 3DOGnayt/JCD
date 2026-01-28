using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Canvas
{
    public class SettingsPanelController : MonoBehaviour
    {
        public event Action Closed;

        [SerializeField] private Button _closeButton;
        [SerializeField] private GameObject _settingsPanel;
        [SerializeField] private GameObject _menuPanel;
        [SerializeField] private RectTransform _panelTransform;
        [SerializeField] private float _slideDuration = 0.25f;
        [SerializeField] private AnimationCurve _slideEase;
        [SerializeField] private float _slideOffset = 600f;

        private Vector2 _shownPosition;
        private Vector2 _hiddenPosition;
        private Coroutine _slideRoutine;
        private bool _positionsCached;

        private void Awake()
        {
            CachePositions();
            if (_closeButton != null)
                _closeButton.onClick.AddListener(CloseSettings);
        }

        private void OnDestroy()
        {
            if (_closeButton != null)
                _closeButton.onClick.RemoveListener(CloseSettings);
        }

        public void OpenSettings()
        {
            CachePositions();
            if (_settingsPanel != null && !_settingsPanel.activeSelf)
                _settingsPanel.SetActive(true);

            if (_panelTransform != null)
                _panelTransform.anchoredPosition = _hiddenPosition;
            StartSlide(_hiddenPosition, _shownPosition, false);
        }

        private void CloseSettings()
        {
            CachePositions();
            var from = _panelTransform != null ? _panelTransform.anchoredPosition : _shownPosition;
            StartSlide(from, _hiddenPosition, true);
            if (_menuPanel != null)
                _menuPanel.SetActive(true);

            Closed?.Invoke();
        }

        public void HideInstantly()
        {
            if (_settingsPanel != null)
                _settingsPanel.SetActive(false);
        }

        private void CachePositions()
        {
            if (_panelTransform == null && _settingsPanel != null)
                _panelTransform = _settingsPanel.GetComponent<RectTransform>();

            if (_panelTransform == null || _positionsCached)
                return;

            _shownPosition = _panelTransform.anchoredPosition;
            _hiddenPosition = _shownPosition + new Vector2(0f, _slideOffset);
            _positionsCached = true;
        }

        private void StartSlide(Vector2 from, Vector2 to, bool deactivateOnComplete)
        {
            if (_panelTransform == null)
                return;

            if (_slideRoutine != null)
                StopCoroutine(_slideRoutine);

            _slideRoutine = StartCoroutine(SlideRoutine(from, to, deactivateOnComplete));
        }

        private IEnumerator SlideRoutine(Vector2 from, Vector2 to, bool deactivateOnComplete)
        {
            var duration = Mathf.Max(0.01f, _slideDuration);
            var ease = _slideEase != null && _slideEase.length > 0
                ? _slideEase
                : AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                var eased = ease.Evaluate(t);
                _panelTransform.anchoredPosition = Vector2.LerpUnclamped(from, to, eased);
                yield return null;
            }

            _panelTransform.anchoredPosition = to;

            if (deactivateOnComplete && _settingsPanel != null)
                _settingsPanel.SetActive(false);
        }
    }
}
