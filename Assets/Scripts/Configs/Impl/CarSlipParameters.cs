using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Car/" + nameof(CarSlipParameters), fileName = nameof(CarSlipParameters))]
    public class CarSlipParameters : ScriptableObject
    {
        [Header("Sideways slip (handbrake)")]
        [SerializeField] private float _handbrakeSidewaysMultiplier = 0.4f;
        [SerializeField] private float _stiffnessEnterSpeed = 5f;
        [SerializeField] private float _stiffnessReturnSpeed = 5f;
        
        public float HandbrakeSidewaysMultiplier => _handbrakeSidewaysMultiplier;
        public float StiffnessEnterSpeed => _stiffnessEnterSpeed;
        public float StiffnessReturnSpeed => _stiffnessReturnSpeed;
    }
}