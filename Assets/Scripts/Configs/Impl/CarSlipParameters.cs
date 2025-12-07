using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Car/" + nameof(CarSlipParameters), fileName = nameof(CarSlipParameters))]
    public class CarSlipParameters : ScriptableObject
    {
        [Header("Sideways slip (handbrake)")]
        [SerializeField] private float _handbrakeSidewaysMultiplier = 0.4f; // 0.0–1.0
        [SerializeField] private float _stiffnessLerpSpeed = 5f;   // скорость перехода

        public float HandbrakeSidewaysMultiplier => _handbrakeSidewaysMultiplier;
        public float StiffnessLerpSpeed => _stiffnessLerpSpeed;
    }
}