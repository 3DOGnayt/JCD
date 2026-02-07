using System;
using System.Collections.Generic;
using Data.Enums;
using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Game/" + nameof(TrainingTimeScoreParameters), fileName = nameof(TrainingTimeScoreParameters), order = 8)]
    public class TrainingTimeScoreParameters : ScriptableObject
    {
        [SerializeField] private List<TrainingTimeScoreEntry> _entries = new List<TrainingTimeScoreEntry>();

        public IReadOnlyList<TrainingTimeScoreEntry> Entries => _entries;

        private TrainingTimeScoreEntry GetEntry(EMap map)
        {
            return _entries.Find(entry => entry.Map == map);
        }

        public TrainingTimeScoreEntry GetOrCreateEntry(EMap map, int segmentCount)
        {
            var entry = GetEntry(map);
            if (entry != null)
                return entry;

            entry = new TrainingTimeScoreEntry
            {
                Map = map,
                BestTotalTime = 0f,
                BestSegmentTimes = CreateSegmentList(segmentCount)
            };
            _entries.Add(entry);
            return entry;
        }

        public void EnsureSegmentCount(TrainingTimeScoreEntry entry, int segmentCount)
        {
            if (entry == null)
                return;

            if (entry.BestSegmentTimes == null)
                entry.BestSegmentTimes = new List<float>();

            while (entry.BestSegmentTimes.Count < segmentCount)
                entry.BestSegmentTimes.Add(0f);
        }

        private List<float> CreateSegmentList(int segmentCount)
        {
            var list = new List<float>();
            for (var i = 0; i < segmentCount; i++)
                list.Add(0f);

            return list;
        }

#if UNITY_EDITOR
        [ContextMenu("Clear Time Values")]
        public void ClearTimeValues()
        {
            foreach (var entry in _entries)
            {
                if (entry == null)
                    continue;

                entry.BestTotalTime = 0f;
                if (entry.BestSegmentTimes == null)
                    continue;

                for (var i = 0; i < entry.BestSegmentTimes.Count; i++)
                    entry.BestSegmentTimes[i] = 0f;
            }

            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }

    [Serializable]
    public class TrainingTimeScoreEntry
    {
        public EMap Map;
        public float BestTotalTime;
        public List<float> BestSegmentTimes = new List<float>();
    }
}
