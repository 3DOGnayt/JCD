using System.Collections.Generic;
using Data.HelperClass;
using Data.Struct;

namespace Services.Impl
{
    public partial class DataService
    {
        private const string TrainingScoresFileName = "training_scores.json";

        public List<TrainingTimeScoreSetup> LoadTrainingTimeScores()
        {
            var fallback = new TrainingTimeScoreSaveData();
            var data = LoadJson(TrainingScoresFileName, fallback);
            return data?.Entries;
        }

        public void SaveTrainingTimeScores(IReadOnlyList<TrainingTimeScoreSetup> entries)
        {
            if (entries == null)
                return;

            var data = new TrainingTimeScoreSaveData();
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry == null)
                    continue;

                data.Entries.Add(entry);
            }

            SaveJson(TrainingScoresFileName, data);
        }
    }
}