using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Car/" + nameof(CarSlipParameters), fileName = nameof(CarSlipParameters))]
    public class CarSlipParameters : ScriptableObject
    {
        [Header("Sideways slip (handbrake)")]
        [SerializeField] private float _handbrakeSidewaysBackMultiplier = 0.6f;
        [SerializeField] private float _backStiffnessEnterSpeed = 5f;
        [SerializeField] private float _BackStiffnessReturnSpeed = 1.5f;
        [Space]
        [SerializeField] private float _handbrakeSidewaysForwardMultiplier = 0.95f;
        [SerializeField] private float _forwardStiffnessEnterSpeed = 5f;
        [SerializeField] private float _forwardStiffnessReturnSpeed = 0.8f;
        
        public float HandbrakeSidewaysBackMultiplier => _handbrakeSidewaysBackMultiplier;
        public float BackStiffnessEnterSpeed => _backStiffnessEnterSpeed;
        public float BackStiffnessReturnSpeed => _BackStiffnessReturnSpeed;

        public float HandbrakeSidewaysForwardMultiplier => _handbrakeSidewaysForwardMultiplier;
        public float ForwardStiffnessEnterSpeed => _forwardStiffnessEnterSpeed;
        public float ForwardStiffnessReturnSpeed => _forwardStiffnessReturnSpeed;
    }
}