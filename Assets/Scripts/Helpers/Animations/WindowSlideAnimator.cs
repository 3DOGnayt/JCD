using DG.Tweening;
using UnityEngine;

namespace Helpers.Animations
{
    [RequireComponent(typeof(RectTransform))]
    public class WindowSlideAnimator : MonoBehaviour, IWindowAnimator
    {
        [SerializeField] private float _duration = 0.25f;
        [SerializeField] private Ease _ease = Ease.OutQuad;
        [SerializeField] private bool _useUnscaledTime = true;

        private RectTransform _rectTransform;
        private Vector2 _baseAnchoredPosition;
        private Tween _currentTween;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _baseAnchoredPosition = _rectTransform.anchoredPosition;
        }

        public void Animate(Vector2 offset)
        {
            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();

            _currentTween?.Kill();
            var targetPosition = _baseAnchoredPosition + offset;

            _currentTween = _rectTransform
                .DOAnchorPos(targetPosition, _duration)
                .SetEase(_ease)
                .SetUpdate(_useUnscaledTime)
                .SetLink(gameObject);
        }
    }
}