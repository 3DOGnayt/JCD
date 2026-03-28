using Scellecs.Morpeh;

namespace Components
{
    public struct RaceLapConfigComponent : IComponent
    {
        public int SelectionCount;
        public int LapCount;
        public int CheckpointsCount;
    }

    public struct RaceLapStateComponent : IComponent
    {
        public int NextCheckpointIndex;
        public int CurrentSelection;
        public int CurrentLap;
        public bool IsLoopStarted;
    }

    public struct RaceLapTriggerEventComponent : IComponent
    {
        public int CheckpointIndex;
        public Entity CarEntity;
    }
}