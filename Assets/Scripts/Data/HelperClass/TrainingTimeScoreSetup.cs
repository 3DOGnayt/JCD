using System;
using System.Collections.Generic;
using Data.Enums;

namespace Data.HelperClass
{
    [Serializable]
    public class TrainingTimeScoreSetup
    {
        public EMap Map;
        public float BestTotalTime;
        public List<float> BestSegmentTimes = new();
    }
}