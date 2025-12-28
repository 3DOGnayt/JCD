using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Car/" + nameof(CarCollisionSlideParameters), fileName = nameof(CarCollisionSlideParameters))]
    public class CarCollisionSlideParameters : ScriptableObject
    {
        [Header("Speed")]
        [SerializeField] private float _minSpeedKmh = 5f;

        [Header("Angle thresholds")]
        [SerializeField] private float _minSlideAngleDeg = 30f;

        [Header("Velocity keep factors")]
        [Range(0f, 1f)] 
        [SerializeField] private float _tangentKeep = 0.9f;

        [Range(0f, 1f)]
        [SerializeField] private float _normalKeep = 0.1f;
        
        [Header("Bounce")]
        [Range(0f, 2f)]
        [SerializeField] private float _bounceFactor = 0.7f;

        public float MinSpeedKmh => _minSpeedKmh;
        public float MinSlideAngleDeg => _minSlideAngleDeg;
        public float TangentKeep => _tangentKeep;
        public float NormalKeep => _normalKeep;
        public float BounceFactor => _bounceFactor;
    }
}