using uk.vroad.api;
using uk.vroad.apk;

namespace uk.vroad.rvr
{
    public class GameStateTransition : AppStateTransition
    {
        public static readonly uk.vroad.rvr.GameStateTransition startSimulation = new uk.vroad.rvr.GameStateTransition(AppState.ReadyToSimulate, GameState.WaitingForTripChoice
            );
        public static readonly uk.vroad.rvr.GameStateTransition chooseTrip = new uk.vroad.rvr.GameStateTransition(GameState.WaitingForTripChoice, GameState.TripChosen
            );
        public static readonly uk.vroad.rvr.GameStateTransition queueRelease = new uk.vroad.rvr.GameStateTransition(GameState.TripChosen, GameState.QueuedForRelease
            );
        public static readonly uk.vroad.rvr.GameStateTransition startNavigating = new uk.vroad.rvr.GameStateTransition(GameState.QueuedForRelease, GameState.Navigating
            );
        public static readonly uk.vroad.rvr.GameStateTransition arrivalAtTargetContinueThisMap = new uk.vroad.rvr.GameStateTransition(GameState.Navigating, GameState
            .WaitingForTripChoice);
        public static readonly uk.vroad.rvr.GameStateTransition arrivalAtTargetLevelUp = new uk.vroad.rvr.GameStateTransition(GameState.Navigating, GameState.LevelUp
            );
        public static readonly uk.vroad.rvr.GameStateTransition escapeFromQuarterWait = new uk.vroad.rvr.GameStateTransition(GameState.WaitingForTripChoice, AppState
            .WaitingForMapChoice);
        public static readonly uk.vroad.rvr.GameStateTransition escapeFromQuarterPlay = new uk.vroad.rvr.GameStateTransition(GameState.Navigating, AppState.WaitingForMapChoice
            );
        public static readonly uk.vroad.rvr.GameStateTransition pauseWhileWaitingForTripChoice = new uk.vroad.rvr.GameStateTransition(GameState.WaitingForTripChoice
            , GameState.PausedAtTripChoice);
        public static readonly uk.vroad.rvr.GameStateTransition pauseWhileNavigating = new uk.vroad.rvr.GameStateTransition(GameState.Navigating, GameState.PausedWhileNavigating
            );
        public static readonly uk.vroad.rvr.GameStateTransition resumeToWaitingForTripChoice = new uk.vroad.rvr.GameStateTransition(GameState.PausedAtTripChoice
            , GameState.WaitingForTripChoice);
        public static readonly uk.vroad.rvr.GameStateTransition resumeToNavigating = new uk.vroad.rvr.GameStateTransition(GameState.PausedWhileNavigating, GameState
            .Navigating);
        public static readonly uk.vroad.rvr.GameStateTransition openCompleteMenu = new uk.vroad.rvr.GameStateTransition(AppState.WaitingForMapChoice, GameState
            .MenuComplete);
        public static readonly uk.vroad.rvr.GameStateTransition closeCompleteMenu = new uk.vroad.rvr.GameStateTransition(GameState.MenuComplete, AppState.WaitingForMapChoice
            );
        public static readonly uk.vroad.rvr.GameStateTransition openBrowser = new uk.vroad.rvr.GameStateTransition(AppState.WaitingForMapChoice, GameState.Browsing
            );
        public static readonly uk.vroad.rvr.GameStateTransition cancelBrowser = new uk.vroad.rvr.GameStateTransition(GameState.Browsing, AppState.WaitingForMapChoice
            );
        public static readonly uk.vroad.rvr.GameStateTransition openBrowseMenu = new uk.vroad.rvr.GameStateTransition(GameState.Browsing, GameState.MenuWhileBrowsing
            );
        public static readonly uk.vroad.rvr.GameStateTransition closeBrowseMenu = new uk.vroad.rvr.GameStateTransition(GameState.MenuWhileBrowsing, GameState.Browsing
            );
        public static readonly uk.vroad.rvr.GameStateTransition startBuildingFromBrowse = new uk.vroad.rvr.GameStateTransition(GameState.Browsing, AppState.CreatingNewMap
            );
        public static readonly uk.vroad.rvr.GameStateTransition startNewLevel = new uk.vroad.rvr.GameStateTransition(GameState.LevelUp, AppState.WaitingForMapChoice
            );
        public static readonly uk.vroad.rvr.GameStateTransition gameOverWhileNavigating = new uk.vroad.rvr.GameStateTransition(GameState.Navigating, GameState.
            GameOver);
        public static readonly uk.vroad.rvr.GameStateTransition removeGameOverMsg = new uk.vroad.rvr.GameStateTransition(GameState.GameOver, AppState.WaitingForMapChoice
            );

        private GameStateTransition(AppState a, AppState b)
            : base(a, b)
        {
        }
        private static bool regG = false;

        public override string ToString()
        {
            if (!regG) 
            {
                regG = true;
                ADbr.RegisterStaticObjectNames(startSimulation);
            }
            return base.ToString();
        }
    }
}
