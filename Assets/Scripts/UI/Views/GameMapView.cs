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

        private MinimapCameraHolder _minimapCamera;

        public void SetMinimapCamera(MinimapCameraHolder minimapCameraInstance)
        {
            _minimapCamera = minimapCameraInstance;
            
            if (_mapImage != null && _minimapCamera != null && _minimapCamera.Camera != null)
                _mapImage.texture = _minimapCamera.Camera.targetTexture;
        }
    }
}