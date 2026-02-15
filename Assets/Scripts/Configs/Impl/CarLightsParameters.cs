using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Car/" + nameof(CarLightsParameters), fileName = nameof(CarLightsParameters))]
    public class CarLightsParameters : ScriptableObject
    {
        [Header("Headlights")] 
        [SerializeField] private float _lowRange = 8f;
        [SerializeField] private float _highRange = 16f;
        [SerializeField] private float _lowIntensity = 1.5f;
        [SerializeField] private float _highIntensity = 2.5f;

        public float LowRange => _lowRange;
        public float HighRange => _highRange;
        public float LowIntensity => _lowIntensity;
        public float HighIntensity => _highIntensity;
    }
}