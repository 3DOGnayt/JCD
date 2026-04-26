using uk.vroad.api.etc;
using uk.vroad.api.events;
using uk.vroad.api.map;
using uk.vroad.apk;

namespace uk.vroad.rvr
{
    public class IncidentGenerator : LSimTimeSec, LSimRewind
    {
        private KList<Incident> incidents = new KList<Incident>();
        private KHashSet<ILane> stoppedLanes = new KHashSet<ILane>();
        private readonly Game game;

        public static uk.vroad.rvr.IncidentGenerator Awake(Game game)
        {
            lock (typeof(IncidentGenerator))
            {
                return new uk.vroad.rvr.IncidentGenerator(game);
            }
        }

        private IncidentGenerator(Game gm)
        {
            game = gm;
            gm.AddEventConsumer(this);
        }

        public virtual bool DeregisterFireMapChange()
        {
            Clear();
            return false;
        }

        public virtual void TimeSec()
        {
            double now = game.Sim().TimeNow();
            if (GameFeatures.IncidentsOn()) 
            {
                if (Rng.NextDouble(Rng.Vein.INCIDENT) * 3600 < GameFeatures.INCIDENTS_PER_HOUR) incidents.Add(new Incident(game));
            }
            stoppedLanes.Clear();
            foreach (Incident incident in incidents)
            {
                if (incident.Active() && incident.HasExpired(now)) incident.Expire(game.Ew());
                if (incident.Active()) incident.AddStoppedLanes(stoppedLanes);
            }
        }

        public virtual void TimeRewind()
        {
            Clear();
        }

        public virtual void Clear()
        {
            EventWrangler ew = game.Ew();
            foreach (Incident incident in incidents)
            {
                incident.Expire(ew);
                ew.FireDynamicObjectDeleted(incident);
            }
            incidents.Clear();
            stoppedLanes.Clear();
        }

        public virtual bool IsStopped(ILane lane)
        {
            return stoppedLanes.Contains(lane);
        }
    }
}
