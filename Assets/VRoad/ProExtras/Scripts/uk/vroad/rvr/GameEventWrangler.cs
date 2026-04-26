using uk.vroad.api.events;
using uk.vroad.apk;

namespace uk.vroad.rvr
{
    public class GameEventWrangler : IEventDistributor
    {
        public static uk.vroad.rvr.GameEventWrangler Awake(Game game)
        {
            lock (typeof(GameEventWrangler))
            {
                return new uk.vroad.rvr.GameEventWrangler(game);
            }
        }

        private GameEventWrangler(Game gm)
        {
            gm.AddEventDistributor(this);
        }
        private static readonly LPlayer[] playerZERO = new LPlayer[0];
        private KSList<LPlayer> playerListeners = new KSList<LPlayer>();

        public virtual void AddEventConsumer(LEvent consumer)
        {
            lock (this)
            {
                if (consumer is LPlayer) playerListeners.Add((LPlayer)consumer);
            }
        }

        public virtual void RemoveEventConsumer(LEvent consumer)
        {
            lock (this)
            {
                if (consumer is LPlayer) playerListeners.Remove((LPlayer)consumer);
            }
        }

        public virtual void FireAlertEvent(AlertEvent alertEvent)
        {
            foreach (LPlayer pl in playerListeners.LockedArray(playerZERO))
            {
                pl.Alert(alertEvent);
            }
        }

        public virtual void FireEnergyEvent(EnergyEvent energyEvent, double deltaJoules)
        {
            foreach (LPlayer pl in playerListeners.LockedArray(playerZERO))
            {
                pl.EnergyEventOccurred(energyEvent, deltaJoules);
            }
        }

        public virtual void FireEnergyChanged(double energyNow)
        {
            foreach (LPlayer pl in playerListeners.LockedArray(playerZERO))
            {
                pl.EnergyChanged(energyNow);
            }
        }

        public virtual void FireSpeedChanged()
        {
            foreach (LPlayer pl in playerListeners.LockedArray(playerZERO))
            {
                pl.SpeedChanged();
            }
        }
    }
}
