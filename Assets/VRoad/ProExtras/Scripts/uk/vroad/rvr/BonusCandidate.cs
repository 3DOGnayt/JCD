using uk.vroad.api.geom;
using uk.vroad.api.map;
using uk.vroad.api.sim;
using uk.vroad.apk;

namespace uk.vroad.rvr
{
    public class BonusCandidate
    {
        public const double SPAWN_TIME = 10.0;
        private const double FIRST_WARNING = 90.0;
        private const double SECOND_WARNING = 30.0;
        private const double NEAR_ENOUGH_PED = 1.0;
        private const double NEAR_ENOUGH_VKL = 4.0;
        private const double TALLER_THAN_MEAN = 0.10;
        public readonly int reward;
        public readonly double dawn;
        public readonly double dusk;
        private readonly Xyzbg location;
        private readonly ICell cell;
        private readonly ILane lane;
        private bool taken;

        public BonusCandidate(IFootpath footpath, double distance, int r, double t0, double t1)
        {
            reward = r;
            dawn = t0;
            dusk = t1;
            Xyzbg fpc = footpath.Position(distance);
            Xyz fwd = fpc.Bearing().UnitVectorXY();
            Xyz loc = fpc;
            location = new Xyzbg(loc, fpc.Bearing(), fpc.GradientFrac());
            cell = footpath.GetCell(distance);
            lane = footpath.GetRunningLane();
        }

        public virtual Xyzbg Location()
        {
            return location;
        }

        public virtual bool IsHidden(double now)
        {
            return now < dawn;
        }

        public virtual bool IsSpawning(double now)
        {
            return now >= dawn && now < dawn + SPAWN_TIME;
        }

        public virtual bool IsAvailable(double now)
        {
            return !taken && now >= dawn + SPAWN_TIME && now < dusk;
        }

        public virtual bool IsTaken()
        {
            return taken;
        }

        public virtual bool IsExpired(double now)
        {
            return !taken && now > dusk;
        }

        public virtual bool ShowFirstWarning(double now)
        {
            return !taken && now > dusk - FIRST_WARNING;
        }

        public virtual bool ShowSecondWarning(double now)
        {
            return !taken && now > dusk - SECOND_WARNING;
        }
        private const double STD_HT = 1.70;

        private bool WillCollectEnergy(IPed p)
        {
            if (Player.IsPlayer(p)) return true;
            double height = p.Height();
            double meanHeight = STD_HT;
            return height > meanHeight + TALLER_THAN_MEAN;
        }

        public virtual bool AnyTakers(Game game)
        {
            if (taken) return false;
            IPed[] peds = cell.Peds();
            foreach (IPed p in peds)
            {
                if (WillCollectEnergy(p) && p.Centre().DistanceXY(location) < NEAR_ENOUGH_PED) 
                {
                    taken = true;
                    if (Player.IsPlayer(p)) game.Pe().ApplyBonus(reward);
                    return true;
                }
            }
            Player player = Player.ActivePlayer();
            if (lane != null && player != null) 
            {
                IVkl vkl = player.Ped().GetVkl();
                if (vkl != null) 
                {
                    double d = vkl.Centre().DistanceXY(location);
                    if (d < NEAR_ENOUGH_VKL) 
                    {
                        taken = true;
                        game.Pe().ApplyBonus(reward);
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
