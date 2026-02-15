using Configs.Impl;
using KoboldUi.Element.View;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Views
{
    public class GameMapView : AUiAnimatedView
    {
        [Header("References")]
        [SerializeField]
        private RawImage _mapImage;

        [Header("Map Settings")]
        [SerializeField]
        private Vector2 _mapWorldSize = new(1000f, 1000f);

        [SerializeField] private Vector2 _mapWorldCenter = Vector2.zero;
        [SerializeField] private float _viewRadiusMeters = 100f;
        [SerializeField] private bool _rotateWithPlayer = true;
        [SerializeField] private bool _clampToMapBounds = true;

        private Transform _player;
        private RectTransform _mapRectTransform;

        public void SetPlayer(Transform player)
        {
            _player = player;
        }

        public void ApplySettings(MapMiniMapSettings settings)
        {
            _mapWorldSize = settings.MapWorldSize;
            _mapWorldCenter = settings.MapWorldCenter;
            _viewRadiusMeters = settings.ViewRadiusMeters;
            _rotateWithPlayer = settings.RotateWithPlayer;
            _clampToMapBounds = settings.ClampToMapBounds;

            if (_mapImage != null && settings.MiniMapTexture != null)
                _mapImage.texture = settings.MiniMapTexture;
        }

        public void UpdateMap()
        {
            if (!isActiveAndEnabled)
                return;

            if (_player == null || _mapImage == null)
                return;

            EnsureMapRectTransform();

            var uvSize = GetUvSize();
            var uvCenter = WorldToMapUv(_player.position);

            if (_clampToMapBounds)
            {
                uvCenter.x = Mathf.Clamp(uvCenter.x, uvSize.x * 0.5f, 1f - uvSize.x * 0.5f);
                uvCenter.y = Mathf.Clamp(uvCenter.y, uvSize.y * 0.5f, 1f - uvSize.y * 0.5f);
            }

            var uvRect = _mapImage.uvRect;
            uvRect.size = uvSize;
            uvRect.position = uvCenter - uvSize * 0.5f;
            _mapImage.uvRect = uvRect;

            if (_mapRectTransform == null)
                return;

            if (_rotateWithPlayer)
            {
                var yaw = _player.eulerAngles.y;
                _mapRectTransform.localRotation = Quaternion.Euler(0f, 0f, yaw);
            }
            else
            {
                _mapRectTransform.localRotation = Quaternion.identity;
            }
        }

        private void EnsureMapRectTransform()
        {
            if (_mapRectTransform != null)
                return;

            _mapRectTransform = _mapImage != null ? _mapImage.rectTransform : null;
        }

        private Vector2 WorldToMapUv(Vector3 worldPos)
        {
            var local = new Vector2(worldPos.x - _mapWorldCenter.x, worldPos.z - _mapWorldCenter.y);
            var u = local.x / Mathf.Max(_mapWorldSize.x, 0.001f) + 0.5f;
            var v = local.y / Mathf.Max(_mapWorldSize.y, 0.001f) + 0.5f;
            return new Vector2(u, v);
        }

        private Vector2 GetUvSize()
        {
            var width = _viewRadiusMeters * 2f / Mathf.Max(_mapWorldSize.x, 0.001f);
            var height = _viewRadiusMeters * 2f / Mathf.Max(_mapWorldSize.y, 0.001f);
            return new Vector2(width, height);
        }
    }
}