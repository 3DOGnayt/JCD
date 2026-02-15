using System.Collections.Generic;
using Configs.Impl;
using UnityEngine;

namespace Services.Impl
{
    public class CrashEffectService : ICrashEffectService
    {
        //TODO: REFACTORING
        private class PoolSlot
        {
            public GameObject GameObject;
            public ParticleSystem ParticleSystem;
            public bool Busy;
            public float ReleaseTime;
        }

        private class CrashInstance
        {
            public int SlotIndex = -1;
        }

        private readonly CarCrashEffectsParameters _parameters;
        private readonly List<PoolSlot> _pool;
        private readonly Dictionary<Rigidbody, CrashInstance> _activeByRb;
        private Transform _root;

        private int _nextSlotIndex = 0;

        public CrashEffectService(CarCrashEffectsParameters parameters)
        {
            _parameters = parameters;
            _pool = new List<PoolSlot>();
            _activeByRb = new Dictionary<Rigidbody, CrashInstance>();

            ResetPool();
        }

        public void ResetPool()
        {
            DestroyRoot();
            _pool.Clear();
            _activeByRb.Clear();
            _nextSlotIndex = 0;

            if (_parameters == null)
            {
                Debug.LogError("CrashEffectService: parameters is null.");
                return;
            }

            if (_parameters.SparkPrefab == null)
            {
                Debug.LogError("CrashEffectService: SparksPrefab is null in CrashEffectsParameters.");
                return;
            }

            CreateRoot();
            InitPool();
        }

        private void CreateRoot()
        {
            var rootGo = new GameObject("CrashEffectsPool");
            _root = rootGo.transform;
            _root.position = Vector3.zero;
            _root.rotation = Quaternion.identity;
        }

        private void DestroyRoot()
        {
            if (_root == null)
                return;

            Object.Destroy(_root.gameObject);
            _root = null;
        }

        private void InitPool()
        {
            var poolSize = Mathf.Max(1, _parameters.PoolSize);

            for (var i = 0; i < poolSize; i++)
            {
                var spark = Object.Instantiate(_parameters.SparkPrefab, _root);
                spark.SetActive(true);

                var particleSystem = spark.GetComponent<ParticleSystem>();
                
                if (particleSystem == null)
                    Debug.LogError("CrashEffectService: SparksPrefab has no ParticleSystem.");
                else
                    particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

                _pool.Add(new PoolSlot
                {
                    GameObject = spark,
                    ParticleSystem = particleSystem,
                    Busy = false,
                    ReleaseTime = 0f
                });
            }
        }

        public void UpdateCrash(Rigidbody rigidbody, bool hasContact, Vector3 contactPoint)
        {
            if (rigidbody == null || _pool == null || _pool.Count == 0)
                return;

            if (!_activeByRb.TryGetValue(rigidbody, out var instance))
            {
                instance = new CrashInstance();
                _activeByRb[rigidbody] = instance;
            }

            var now = Time.time;

            if (!hasContact)
            {
                HandleNoContact(instance, now);
                return;
            }

            HandleContact(instance, contactPoint, now);
        }

        private void HandleNoContact(CrashInstance instance, float now)
        {
            if (instance.SlotIndex < 0)
                return;

            var idx = instance.SlotIndex;
            if (idx >= 0 && idx < _pool.Count)
            {
                var slot = _pool[idx];
                if (slot.ParticleSystem != null)
                    slot.ParticleSystem.Stop(false, ParticleSystemStopBehavior.StopEmitting);

                slot.Busy = true;
                slot.ReleaseTime = now + _parameters.ReleaseDelaySeconds;
            }

            instance.SlotIndex = -1;
        }

        private void HandleContact(CrashInstance instance, Vector3 contactPoint, float now)
        {
            if (instance.SlotIndex < 0)
                StartEffect(instance, contactPoint, now);
            else
                UpdateEffectPosition(instance, contactPoint);
        }

        private void StartEffect(CrashInstance instance, Vector3 contactPoint, float now)
        {
            var slotIndex = AcquireFreeSlot(now);
            if (slotIndex < 0)
                return;

            var slot = _pool[slotIndex];
            if (slot.ParticleSystem == null || slot.GameObject == null)
                return;

            instance.SlotIndex = slotIndex;

            var transform = slot.GameObject.transform;
            transform.position = contactPoint + Vector3.up * _parameters.HeightOffset;

            slot.ParticleSystem.Clear(true);
            slot.ParticleSystem.Play(true);
        }

        private void UpdateEffectPosition(CrashInstance instance, Vector3 contactPoint)
        {
            var index = instance.SlotIndex;
            if (index < 0 || index >= _pool.Count)
            {
                instance.SlotIndex = -1;
                return;
            }

            var slot = _pool[index];
            if (slot.GameObject == null)
            {
                instance.SlotIndex = -1;
                return;
            }

            slot.GameObject.transform.position = contactPoint + Vector3.up * _parameters.HeightOffset;

            if (slot.ParticleSystem != null && !slot.ParticleSystem.isPlaying)
                slot.ParticleSystem.Play(true);
        }

        private int AcquireFreeSlot(float now)
        {
            var count = _pool.Count;
            if (count == 0)
                return -1;

            for (var i = 0; i < count; i++)
            {
                var idx = (_nextSlotIndex + i) % count;
                var slot = _pool[idx];

                if (slot == null)
                    continue;

                if (slot.Busy && now < slot.ReleaseTime)
                    continue;

                slot.Busy = true;
                slot.ReleaseTime = float.PositiveInfinity;

                _nextSlotIndex = (idx + 1) % count;
                return idx;
            }

            return -1;
        }
    }
}