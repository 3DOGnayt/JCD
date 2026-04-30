using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Car/" + nameof(CarEngineAudioParameters), fileName = nameof(CarEngineAudioParameters))]
    public class CarEngineAudioParameters : ScriptableObject
    {
        [Header("Pitch")]
        [SerializeField] private float _lowPitchMin = 1f;
        [SerializeField] private float _lowPitchMax = 1.8f;
        [SerializeField] private float _medPitchMin = 0.4f;
        [SerializeField] private float _medPitchMax = 1.6f;
        [SerializeField] private float _highPitchMin = -1.6f;
        [SerializeField] private float _highPitchMax = 1.5f;

        [Header("Volume")]
        [SerializeField] private float _fadeSpeed = 6f;
        [SerializeField] private float _lowVolumeScale = 0.8f;

        [Header("Bands")]
        [SerializeField] private float _bandLowMax = 0.4f;
        [SerializeField] private float _bandMedMax = 0.75f;

        [Header("Speed")]
        [SerializeField] private float _defaultMaxSpeedKmh = 200f;

        public float LowPitchMin => _lowPitchMin;
        public float LowPitchMax => _lowPitchMax;
        public float MedPitchMin => _medPitchMin;
        public float MedPitchMax => _medPitchMax;
        public float HighPitchMin => _highPitchMin;
        public float HighPitchMax => _highPitchMax;
        public float FadeSpeed => _fadeSpeed;
        public float LowVolumeScale => _lowVolumeScale;
        public float BandLowMax => _bandLowMax;
        public float BandMedMax => _bandMedMax;
        public float DefaultMaxSpeedKmh => _defaultMaxSpeedKmh;
    }
}