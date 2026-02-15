using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Car/" + nameof(CarSkidmarksParameters), fileName = nameof(CarSkidmarksParameters))]
    public class CarSkidmarksParameters : ScriptableObject
    {
        [Header("Mesh / Visual")]
        [SerializeField] private Material _material;
        [SerializeField] private int _maxMarks = 255;
        [SerializeField] private float _markWidth = 0.35f;
        [SerializeField] private float _groundOffset = 0.02f;
        [SerializeField] private float _minDistance = 0.5f;
        [SerializeField] private float _maxOpacity = 1.0f;
        [SerializeField] private float _forwardOffsetMax = 1.5f;
        [SerializeField] private float _referenceSpeedMps = 30f;
        [Space] 
        [Header("Fade")]
        [SerializeField] private float _fadeInSpeed = 8f;
        [SerializeField] private float _fadeOutSpeed = 6f;
        [Range(0f, 1f)] 
        [SerializeField] private float _minVisibleAlpha = 0.3f;

        public Material Material => _material;
        public int MaxMarks => _maxMarks;
        public float MarkWidth => _markWidth;
        public float GroundOffset => _groundOffset;
        public float MinDistance => _minDistance;
        public float MaxOpacity => _maxOpacity;
        public float ForwardOffsetMax => _forwardOffsetMax;
        public float ReferenceSpeedMps => _referenceSpeedMps;

        public float FadeInSpeed => _fadeInSpeed;
        public float FadeOutSpeed => _fadeOutSpeed;
        public float MinVisibleAlpha => _minVisibleAlpha;
    }
}