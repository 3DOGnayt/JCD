namespace Data.Struct
{
    public readonly struct RaceLapRecord
    {
        public int LapIndex { get; }
        public float LapTime { get; }
        public float TotalTime { get; }

        public RaceLapRecord(int lapIndex, float lapTime, float totalTime)
        {
            LapIndex = lapIndex;
            LapTime = lapTime;
            TotalTime = totalTime;
        }
    }
}
