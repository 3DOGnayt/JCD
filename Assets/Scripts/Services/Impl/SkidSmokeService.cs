using System.Collections.Generic;
using Configs.Impl;
using UnityEngine;

namespace Services.Impl
{
    public sealed class SkidSmokeService : ISkidSmokeService
    {
        //TODO: REFACTORING
        private class PoolSlot
        {
            public GameObject GameObject;
            public ParticleSystem ParticleSystem;
            public bool Busy;
            public float ReleaseTime;
        }

        private class SkidInstance
        {
            public int Id;
            public int[] SlotIndices;
        }

        private readonly CarSkidSmokeParameters _parameters;
        private readonly List<PoolSlot> _pool;
        private readonly Dictionary<int, SkidInstance> _activeSkids;
        private Transform _root;

        private int _nextId = 1;

        public SkidSmokeService(CarSkidSmokeParameters parameters)
        {
            _parameters = parameters;
            _pool = new List<PoolSlot>();
            _activeSkids = new Dictionary<int, SkidInstance>();

            ResetPool();
        }

        public void ResetPool()
        {
            DestroyRoot();
            _pool.Clear();
            _activeSkids.Clear();
            _nextId = 1;

            if (_parameters == null)
            {
                Debug.LogError("SkidSmokeService: parameters is null.");
                return;
            }

            if (_parameters.SmokePrefab == null)
            {
                Debug.LogError("SkidSmokeService: SmokePrefab is null in SkidSmokeParameters.");
                return;
            }

            CreateRoot();
            InitPool();
        }

        public int BeginSkid(List<Vector3> wheelPositions)
        {
            if (!CanStartSkid(wheelPositions))
                return -1;

            var emittersNeeded = GetEmittersNeeded(wheelPositions.Count);
            if (emittersNeeded <= 0)
                return -1;

            var now = Time.time;
            var chosenSlots = CollectFreeSlots(emittersNeeded, now);
            if (chosenSlots.Count == 0)
                return -1;

            var instance = CreateSkidInstance(chosenSlots);

            InitSkidSlots(instance, wheelPositions);

            _activeSkids[instance.Id] = instance;
            return instance.Id;
        }
        
        public void UpdateSkid(int handle, List<Vector3> wheelPositions)
        {
            if (handle < 0 || wheelPositions == null || wheelPositions.Count == 0)
                return;

            if (!_activeSkids.TryGetValue(handle, out var instance))
                return;

            for (var i = 0; i < instance.SlotIndices.Length; i++)
            {
                if (i >= wheelPositions.Count)
                    break;

                var slotIndex = instance.SlotIndices[i];
                var slot = _pool[slotIndex];

                if (slot.GameObject == null)
                    continue;

                var wheelPos = wheelPositions[i];
                slot.GameObject.transform.position =
                    wheelPos + Vector3.up * _parameters.HeightOffset;
            }
        }

        public void EndSkid(int handle)
        {
            if (handle < 0)
                return;

            if (!_activeSkids.TryGetValue(handle, out var instance))
                return;

            var now = Time.time;
            var delay = Mathf.Max(0f, _parameters.ReleaseDelay);

            foreach (var slotIndex in instance.SlotIndices)
            {
                var slot = _pool[slotIndex];
                if (slot.ParticleSystem != null)
                    slot.ParticleSystem.Stop(false, ParticleSystemStopBehavior.StopEmitting);

                slot.Busy = true;
                slot.ReleaseTime = now + delay;
            }

            _activeSkids.Remove(handle);
        }

        private bool CanStartSkid(List<Vector3> wheelPositions)
        {
            if (wheelPositions == null || wheelPositions.Count == 0)
                return false;

            if (_pool == null || _pool.Count == 0)
                return false;

            return true;
        }

        private int GetEmittersNeeded(int wheelsCount)
        {
            return Mathf.Min(_parameters.EmittersPerSkid, Mathf.Max(0, wheelsCount));
        }

        private List<int> CollectFreeSlots(int emittersNeeded, float now)
        {
            var chosenSlots = new List<int>(emittersNeeded);

            for (var i = 0; i < _pool.Count && chosenSlots.Count < emittersNeeded; i++)
            {
                var slot = _pool[i];

                if (slot.Busy && now < slot.ReleaseTime)
                    continue;

                slot.Busy = false;
                chosenSlots.Add(i);
            }

            return chosenSlots;
        }

        private SkidInstance CreateSkidInstance(List<int> slotIndices)
        {
            var id = _nextId++;

            return new SkidInstance
            {
                Id = id,
                SlotIndices = slotIndices.ToArray()
            };
        }

        private void InitSkidSlots(SkidInstance instance, List<Vector3> wheelPositions)
        {
            for (var i = 0; i < instance.SlotIndices.Length; i++)
            {
                var slotIndex = instance.SlotIndices[i];
                var slot = _pool[slotIndex];

                if (slot.ParticleSystem == null)
                    continue;

                var wheelPos = wheelPositions[Mathf.Min(i, wheelPositions.Count - 1)];
                var t = slot.GameObject.transform;

                t.position = wheelPos + Vector3.up * _parameters.HeightOffset;

                if (!slot.ParticleSystem.isPlaying)
                    slot.ParticleSystem.Play();

                slot.Busy = true;
                slot.ReleaseTime = float.PositiveInfinity;
            }
        }

        private void CreateRoot()
        {
            var rootGo = new GameObject("SkidSmokePool");
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
            _pool.Capacity = Mathf.Max(_pool.Capacity, poolSize);

            for (var i = 0; i < poolSize; i++)
            {
                var go = Object.Instantiate(_parameters.SmokePrefab, _root);
                go.SetActive(true);

                var ps = go.GetComponent<ParticleSystem>();
                if (ps == null)
                    Debug.LogError("SkidSmokeService: SmokePrefab has no ParticleSystem.");

                _pool.Add(new PoolSlot
                {
                    GameObject = go,
                    ParticleSystem = ps,
                    Busy = false,
                    ReleaseTime = 0f
                });
            }
        }
    }
}