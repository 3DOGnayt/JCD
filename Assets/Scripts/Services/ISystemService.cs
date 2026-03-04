namespace Services
{
    public interface ISystemService
    {
        void RegisterInitialSystems();
        void RegisterDelayedSystems();
    }
}
