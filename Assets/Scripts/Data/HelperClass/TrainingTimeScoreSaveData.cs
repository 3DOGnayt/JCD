using System;
using System.Collections.Generic;

namespace Data.HelperClass
{
    [Serializable]
    public class TrainingTimeScoreSaveData
    {
        public List<TrainingTimeScoreSetup> Entries = new();
    }
}