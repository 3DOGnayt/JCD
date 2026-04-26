using uk.vroad.api;
using uk.vroad.apk;

namespace uk.vroad.rvr
{
    public class GameStateMachine : AppStateMachine
    {
        public static uk.vroad.rvr.GameStateMachine Awake(Game gm)
        {
            lock (typeof(GameStateMachine))
            {
                return new uk.vroad.rvr.GameStateMachine(gm);
            }
        }

        private GameStateMachine(Game gm)
            : base(gm)
        {
        }

        public override void Resume()
        {
            if (CurrentState() == GameState.PausedWhileNavigating) MakeTransition(GameStateTransition.resumeToNavigating);
            else if (CurrentState() == GameState.PausedAtTripChoice) MakeTransition(GameStateTransition.resumeToWaitingForTripChoice);
        }

        public override void Pause()
        {
            if (CurrentState() == GameState.Navigating) MakeTransition(GameStateTransition.pauseWhileNavigating);
            else if (CurrentState() == GameState.WaitingForTripChoice) MakeTransition(GameStateTransition.pauseWhileWaitingForTripChoice);
        }

        public override bool IsPaused()
        {
            if (CurrentState() == GameState.PausedWhileNavigating) return true;
            if (CurrentState() == GameState.PausedAtTripChoice) return true;
            return false;
        }

        public override bool CanPauseFromCurrentState()
        {
            if (CurrentState() == GameState.Navigating) return true;
            else if (CurrentState() == GameState.WaitingForTripChoice) return true;
            return false;
        }

        public override bool InFixedUpdateCountdownState()
        {
            if (CurrentState() == GameState.WaitingForTripChoice) return true;
            if (CurrentState() == GameState.TripChosen) return true;
            if (CurrentState() == GameState.QueuedForRelease) return true;
            if (CurrentState() == GameState.Navigating) return true;
            if (CurrentState() == GameState.GameOver) return true;
            return false;
        }
    }
}
