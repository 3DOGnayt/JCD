using System;
using System.Collections.Generic;
using Data.HelperClass;

namespace Data.Struct
{
    [Serializable]
    public class TrainingTimeScoreSaveData
    {
        public List<TrainingTimeScoreSetup> Entries = new();
    }
}