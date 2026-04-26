using uk.vroad.api;
using uk.vroad.apk;

namespace uk.vroad.rvr
{
    public class GameState : AppState
    {
        public static readonly GameState WaitingForTripChoice = new GameState();
        public static readonly GameState PausedAtTripChoice = new GameState();
        public static readonly GameState TripChosen = new GameState();
        public static readonly GameState QueuedForRelease = new GameState();
        public static readonly GameState Navigating = new GameState();
        public static readonly GameState PausedWhileNavigating = new GameState();
        public static readonly GameState MenuComplete = new GameState();
        public static readonly GameState Browsing = new GameState();
        public static readonly GameState MenuWhileBrowsing = new GameState();
        public static readonly GameState LevelUp = new GameState();
        public static readonly GameState GameOver = new GameState();
        private static bool regG = false;

        public override string ToString()
        {
            if (!regG) 
            {
                regG = true;
                ADbr.RegisterStaticObjectNames(TripChosen);
            }
            return base.ToString();
        }
    }
}
