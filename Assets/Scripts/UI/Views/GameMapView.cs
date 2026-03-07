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

        private MinimapCameraController _minimapCameraInstance;

        public void SetMinimapCamera(MinimapCameraController minimapCameraInstance)
        {
            _minimapCameraInstance = minimapCameraInstance;
            
            if (_mapImage != null && _minimapCameraInstance != null && _minimapCameraInstance.Camera != null)
                _mapImage.texture = _minimapCameraInstance.Camera.targetTexture;
        }
    }
}