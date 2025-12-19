using UnityEngine;

namespace Data
{
    [CreateAssetMenu(menuName = "Car/" + nameof(SkidSmokeParameters), fileName = nameof(SkidSmokeParameters))]
    public class SkidSmokeParameters : ScriptableObject
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _smokePrefab;

        [Header("Axles")]
        [SerializeField] private bool _enableFrontSmoke = true;
        [SerializeField] private bool _enableBackSmoke  = true;
        
        [Header("Pool")]
        [SerializeField] private int _poolSize = 12;
        [SerializeField] private int _emittersPerSkid = 4;

        [Header("Placement")]
        [SerializeField] private float _heightOffset = 0.05f;

        [Header("Reuse")]
        [SerializeField] private float _releaseDelay = 1.5f; 

        public GameObject SmokePrefab => _smokePrefab;

        public bool EnableBackSmoke  => _enableBackSmoke;
        public bool EnableFrontSmoke => _enableFrontSmoke;
        
        public int PoolSize => _poolSize;
        public int EmittersPerSkid => _emittersPerSkid;
        public float HeightOffset => _heightOffset;
        public float ReleaseDelay => _releaseDelay;
    }
}