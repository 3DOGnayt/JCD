using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Car/CrashEffectsParameters", fileName = "CrashEffectsParameters")]
    public class CrashEffectsParameters : ScriptableObject
    {
        [SerializeField] private GameObject _sparkPrefab;
        [SerializeField] private int _poolSize = 10;
        [Space]
        [SerializeField] private float _releaseDelaySeconds = 0.25f;
        [SerializeField] private float _heightOffset = 0.05f;
        
        public GameObject SparkPrefab => _sparkPrefab;
        public int PoolSize => _poolSize;
        public float ReleaseDelaySeconds => _releaseDelaySeconds;
        public float HeightOffset => _heightOffset;
    }
}