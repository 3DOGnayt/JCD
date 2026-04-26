using uk.vroad.api.etc;
using uk.vroad.api.events;
using uk.vroad.api.map;
using uk.vroad.api.sim;
using uk.vroad.api.str;
using uk.vroad.apk;

namespace uk.vroad.rvr
{
    public class VialGenerator : LSimRunning, LSimRewind, LBotArrive
    {
        private IPedZone vialZone;
        private readonly Game game;

        public static uk.vroad.rvr.VialGenerator Awake(Game game)
        {
            lock (typeof(VialGenerator))
            {
                return new uk.vroad.rvr.VialGenerator(game);
            }
        }

        private VialGenerator(Game gm)
        {
            game = gm;
            gm.AddEventConsumer(this);
        }

        private void VialZone(IPedZone pz)
        {
            vialZone = pz;
        }

        public virtual IPedZone VialZone()
        {
            return vialZone;
        }

        public virtual void Clear()
        {
            VialZone(null);
        }

        public virtual bool DeregisterFireMapChange()
        {
            return false;
        }

        public virtual void TimeRewind()
        {
        }

        public virtual void Running(bool isRunning)
        {
            if (isRunning && VialZone() == null) 
            {
                ICouple vzc = game.Map().Couple(SF.VIAL_ZONE);
                if (vzc != null) VialZone(game.Map().PedZone(vzc.Value()));
                if (VialZone() == null) 
                {
                    bool allowEdgeVial = game.Pe().CurrentLevel() >= PlayerEnergy.VIALS_ON_EDGE_FROM_LEVEL;
                    KList<IPedZone> possibleZones = new KList<IPedZone>();
                    foreach (IPedZone pz in game.Map().PedZones())
                    {
                        if (pz.ArrivalBranches().Length > 0) 
                        {
                            if (!pz.IsOnEdge() || allowEdgeVial) possibleZones.Add(pz);
                        }
                    }
                    int npz = possibleZones.Count;
                    if (npz > 0) 
                    {
                        int pzi = Rng.NextInt(Rng.Vein.VIAL, npz);
                        VialZone(possibleZones[pzi]);
                    }
                }
            }
        }

        public virtual void Arrive(IBot bot, IZone z)
        {
            if (bot is IPed && Player.IsPlayer((IPed)bot) && z == VialZone()) 
            {
                PlayerEnergy pe = game.Pe();
                bool newVial = pe.CollectVial();
            }
        }
    }
}
