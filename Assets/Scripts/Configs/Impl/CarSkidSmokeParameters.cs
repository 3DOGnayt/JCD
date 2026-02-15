using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Car/" + nameof(CarSkidSmokeParameters), fileName = nameof(CarSkidSmokeParameters))]
    public class CarSkidSmokeParameters : ScriptableObject
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _smokePrefab;

        [Header("Axles")]
        [SerializeField] private bool _enableFrontSmoke;
        [SerializeField] private bool _enableBackSmoke  = true;
        
        [Header("Pool")]
        [SerializeField] private int _poolSize = 32;
        [SerializeField] private int _emittersPerSkid = 4;

        [Header("Placement")]
        [SerializeField] private float _heightOffset = -0.5f;

        [Header("Reuse")]
        [SerializeField] private float _releaseDelay = 2f; 

        public GameObject SmokePrefab => _smokePrefab;

        public bool EnableBackSmoke  => _enableBackSmoke;
        public bool EnableFrontSmoke => _enableFrontSmoke;
        
        public int PoolSize => _poolSize;
        public int EmittersPerSkid => _emittersPerSkid;
        public float HeightOffset => _heightOffset;
        public float ReleaseDelay => _releaseDelay;
    }
}