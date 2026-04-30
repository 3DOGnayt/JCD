using Cameras;
using KoboldUi.Element.View;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Views
{
    public class GameMapView : AUiAnimatedView
    {
        [Header("References")]
        [SerializeField] private RawImage _mapImage;
        [SerializeField] private RectTransform _enemyDot;

        private MinimapCameraHolder _minimapCamera;

        public RectTransform MapRect => _mapImage != null ? _mapImage.rectTransform : null;

        public void SetMinimapCamera(MinimapCameraHolder minimapCameraInstance)
        {
            _minimapCamera = minimapCameraInstance;
            
            if (_mapImage != null && _minimapCamera != null && _minimapCamera.Camera != null)
                _mapImage.texture = _minimapCamera.Camera.targetTexture;
        }

        public void SetEnemyPosition(Vector2 anchoredPosition)
        {
            if (_enemyDot == null)
                return;

            _enemyDot.anchoredPosition = anchoredPosition;
        }

        public void SetEnemyVisible(bool isVisible)
        {
            if (_enemyDot == null)
                return;

            _enemyDot.gameObject.SetActive(isVisible);
        }
    }
}