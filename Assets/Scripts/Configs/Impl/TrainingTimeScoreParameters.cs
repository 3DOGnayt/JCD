using System.Collections.Generic;
using Data.Enums;
using Data.HelperClass;
using UnityEngine;

namespace Configs.Impl
{
    [CreateAssetMenu(menuName = "Game/" + nameof(TrainingTimeScoreParameters), fileName = nameof(TrainingTimeScoreParameters), order = 8)]
    public class TrainingTimeScoreParameters : ScriptableObject
    {
        [SerializeField] private List<TrainingTimeScoreSetup> _entries = new();

        public IReadOnlyList<TrainingTimeScoreSetup> Entries => _entries;

        public void ReplaceEntries(List<TrainingTimeScoreSetup> entries)
        {
            _entries = entries != null ? new List<TrainingTimeScoreSetup>(entries) : new List<TrainingTimeScoreSetup>();
        }

        private TrainingTimeScoreSetup GetEntry(EMap map)
        {
            return _entries.Find(entry => entry.Map == map);
        }

        public TrainingTimeScoreSetup GetOrCreateEntry(EMap map, int segmentCount)
        {
            var entry = GetEntry(map);
            if (entry != null)
                return entry;

            entry = new TrainingTimeScoreSetup
            {
                Map = map,
                BestTotalTime = 0f,
                BestSegmentTimes = CreateSegmentList(segmentCount)
            };
            _entries.Add(entry);
            return entry;
        }

        public void EnsureSegmentCount(TrainingTimeScoreSetup setup, int segmentCount)
        {
            if (setup == null)
                return;

            if (setup.BestSegmentTimes == null)
                setup.BestSegmentTimes = new List<float>();

            while (setup.BestSegmentTimes.Count < segmentCount)
                setup.BestSegmentTimes.Add(0f);
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
}