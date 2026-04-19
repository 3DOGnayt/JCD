namespace Data.Struct
{
    public readonly struct RaceLapRecordEntry
    {
        public int LapIndex { get; }
        public float LapTime { get; }
        public float TotalTime { get; }

        public RaceLapRecordEntry(int lapIndex, float lapTime, float totalTime)
        {
            LapIndex = lapIndex;
            LapTime = lapTime;
            TotalTime = totalTime;
        }
    }
}