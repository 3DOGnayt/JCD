using uk.vroad.api.etc;
using uk.vroad.api.events;
using uk.vroad.api.geom;
using uk.vroad.api.map;
using uk.vroad.api.sim;
using uk.vroad.apk;

namespace uk.vroad.rvr
{
    public class Incident
    {
        private const int MIN_INCIDENT_DURATION = 3 * 60;
        private const int MAX_INCIDENT_DURATION = 5 * 60;
        private const int MIN_SIG_JUNCS = 4;
        private const int MIN_VKLS = 10;
        private readonly IVkl vkl;
        private readonly IJunctionControl junctionControl;
        private readonly double dawn;
        private readonly double dusk;
        private bool active;

        public Incident(Game game)
        {
            Rng.Vein vein = Rng.Vein.INCIDENT;
            dawn = game.Sim().TimeNow();
            dusk = dawn + MIN_INCIDENT_DURATION + Rng.NextInt(vein, MAX_INCIDENT_DURATION - MIN_INCIDENT_DURATION);
            IJunctionControl[] jca = game.Map().JunctionControls();
            int nj = jca.Length;
            int p = Rng.NextInt(vein, 100);
            IVkl vBreakdown = null;
            if (p < GameFeatures.INCIDENT_PROBABILITY_BREAKDOWN || nj <= MIN_SIG_JUNCS) 
            {
                IDrv[] ka = game.Sim().Drvs();
                IBus[] ba = game.Sim().Buses();
                int nv = ka.Length + ba.Length;
                if (nv >= MIN_VKLS) 
                {
                    int vi = Rng.NextInt(vein, nv);
                    IVkl v = vi < ka.Length ? (IVkl)ka[vi] : (IVkl)ba[vi - ka.Length];
                    if (v is ITaxi && Player.IsPlayerTaxi((ITaxi)v)) vBreakdown = null;
                    else if (v.IsParked()) vBreakdown = null;
                    else vBreakdown = v;
                }
            }
            if (vBreakdown != null) 
            {
                vkl = vBreakdown;
                vkl.SetStopped(true);
                vkl.SetSpeed(0);
                active = true;
                junctionControl = null;
            }
            else if (nj > MIN_SIG_JUNCS) 
            {
                vkl = null;
                int ji = Rng.NextInt(vein, nj);
                junctionControl = jca[ji];
                junctionControl.SetStopped(true);
                active = true;
            }
            else 
            {
                vkl = null;
                junctionControl = null;
                active = false;
            }
            if (active) game.Ew().FireDynamicObjectCreated(this);
        }

        public virtual bool HasExpired(double now)
        {
            return now > dusk;
        }

        public virtual void Expire(EventWrangler ew)
        {
            if (vkl != null) vkl.SetStopped(false);
            else if (junctionControl != null) junctionControl.SetStopped(false);
            active = false;
            ew.FireDynamicObjectDeleted(this);
        }

        public virtual Xyzbg Location()
        {
            if (vkl != null) return new Xyzbg(vkl.Centre(), vkl.Forward().AsBearing(), 0);
            else if (junctionControl != null) return new Xyzbg(junctionControl.GetJunctions()[0].Location(), Angle.A0, 0);
            else return Xyzbg.ZERO_XYZBG;
        }

        public virtual TimeHMS TimeToDusk(double now)
        {
            return new TimeHMS(dusk - now, TimeHMS.MMSS);
        }

        public virtual bool IsVehicleBreakdown()
        {
            return vkl != null;
        }

        public virtual bool Active()
        {
            return active;
        }

        public virtual void AddStoppedLanes(KHashSet<ILane> stoppedLanes)
        {
            if (vkl != null) 
            {
                ILane vLane = vkl.GetLane();
                if (vLane != null) 
                {
                    AddLaneAndAllStoppedIncoming(stoppedLanes, vLane);
                    IRoad road = vLane.GetRoad();
                    foreach (ILane roadLane in road.GetLanes())
                    {
                        if (roadLane != vLane) AddLaneIfLeaderBlocked(stoppedLanes, roadLane);
                    }
                }
            }
            else if (junctionControl != null) 
                {
                    foreach (IJunction jn in junctionControl.GetJunctions())
                    {
                        for (int ei = 1; ei <= jn.Entries(); ei++)
                        {
                            IRoad entry = jn.GetEntry(ei);
                            foreach (ILane laneI in entry.GetLanes())
                            {
                                AddLaneAndAllStoppedIncoming(stoppedLanes, laneI);
                            }
                        }
                    }
                }
        }

        private void AddLaneAndAllStoppedIncoming(KHashSet<ILane> stoppedLanes, ILane laneO)
        {
            if (stoppedLanes.Contains(laneO)) return;
            stoppedLanes.Add(laneO);
            foreach (IStreme s in laneO.GetStremeI())
            {
                IRoad roadI = s.GetLaneIn().GetRoad();
                foreach (ILane laneI in roadI.GetLanes())
                {
                    AddLaneIfLeaderBlocked(stoppedLanes, laneI);
                }
            }
        }

        private void AddLaneIfLeaderBlocked(KHashSet<ILane> stoppedLanes, ILane lane)
        {
            IVkl v = lane.GetFirstVkl();
            if (v == null) return;
            if (v.IsBlockedByLeader() && stoppedLanes.Contains(v.GetLeaderVkl().GetLane())) AddLaneAndAllStoppedIncoming(stoppedLanes, lane);
        }
    }
}
