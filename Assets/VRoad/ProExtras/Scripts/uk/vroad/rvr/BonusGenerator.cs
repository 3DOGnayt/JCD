using uk.vroad.api;
using uk.vroad.api.etc;
using uk.vroad.api.events;
using uk.vroad.api.map;
using uk.vroad.apk;

namespace uk.vroad.rvr
{
    public class BonusGenerator : LAppState, LSimTimeStep
    {
        public static readonly int[] REWARDS = new int[] { 50, 250, 1000, 2000 };
        private const int N_BONUS_PER_HOUR = 60;
        private static readonly double[] REWARD_P = new double[] { 0.748, 0.200, 0.050, 0.002 };
        private const double MIN_BONUS_DAY = 600.0;
        private const double MAX_BONUS_DAY = 900.0;
        private const double START_BEFORE = 300.0;
        private BonusCandidate[] all;
        private KList<BonusCandidate> hidden = new KList<BonusCandidate>();
        private KList<BonusCandidate> spawning = new KList<BonusCandidate>();
        private KList<BonusCandidate> available = new KList<BonusCandidate>();
        private KList<BonusCandidate> taken = new KList<BonusCandidate>();
        private KList<BonusCandidate> expired = new KList<BonusCandidate>();
        private readonly Game game;

        public static uk.vroad.rvr.BonusGenerator Awake(Game game)
        {
            lock (typeof(BonusGenerator))
            {
                return new uk.vroad.rvr.BonusGenerator(game);
            }
        }

        private BonusGenerator(Game gm)
        {
            game = gm;
            gm.AddEventConsumer(this);
        }

        public virtual bool DeregisterFireMapChange()
        {
            Clear();
            return false;
        }

        public virtual void AppStateChanged(AppStateTransition transition)
        {
            if (transition.after == AppState.ReadyToSimulate) CreateBonusCandidates();
            else if (transition.after == GameState.GameOver) Clear();
        }

        public virtual void TimeStep()
        {
            if (!GameFeatures.BonusOn()) return;
            double now = game.Sim().TimeNow();
            EventWrangler ew = game.Ew();
            KList<BonusCandidate> removals = new KList<BonusCandidate>();
            foreach (BonusCandidate bch in hidden)
            {
                if (!bch.IsHidden(now)) 
                {
                    removals.Add(bch);
                    spawning.Add(bch);
                    ew.FireDynamicObjectCreated(bch);
                }
            }
            foreach (BonusCandidate bcs in spawning)
            {
                if (!bcs.IsSpawning(now)) 
                {
                    removals.Add(bcs);
                    available.Add(bcs);
                }
            }
            foreach (BonusCandidate bca in available)
            {
                if (bca.IsTaken() || bca.AnyTakers(game)) 
                {
                    removals.Add(bca);
                    taken.Add(bca);
                    ew.FireDynamicObjectDeleted(bca);
                }
                else if (bca.IsExpired(now)) 
                {
                    removals.Add(bca);
                    expired.Add(bca);
                    ew.FireDynamicObjectDeleted(bca);
                }
            }
            foreach (BonusCandidate bcr in removals)
            {
                if (!bcr.IsHidden(now)) hidden.Remove(bcr);
                if (!bcr.IsSpawning(now)) spawning.Remove(bcr);
                if (!bcr.IsAvailable(now)) available.Remove(bcr);
            }
        }

        public virtual void Clear()
        {
            if (all == null) return;
            EventWrangler ew = game.Ew();
            foreach (BonusCandidate bc in spawning)
            {
                ew.FireDynamicObjectDeleted(bc);
            }
            foreach (BonusCandidate bc_1 in available)
            {
                ew.FireDynamicObjectDeleted(bc_1);
            }
            all = null;
            hidden.Clear();
            spawning.Clear();
            available.Clear();
            taken.Clear();
            expired.Clear();
        }

        private int Reward(double p)
        {
            double[] rewardP = REWARD_P;
            double acc = 0;
            for (int i = 0; i < rewardP.Length && i < REWARDS.Length; i++)
            {
                acc += rewardP[i];
                if (p < acc) return REWARDS[i];
            }
            return REWARDS[REWARDS.Length - 1];
        }

        private void CreateBonusCandidates()
        {
            if (all != null) return;
            if (!GameFeatures.BonusOn()) return;
            KList<IFootpath> suitableFP = new KList<IFootpath>();
            foreach (IFootpath fp in game.Map().Footpaths())
            {
                if (fp.TiesC().Length == 0 || fp.TiesD().Length == 0) continue;
                suitableFP.Add(fp);
            }
            int nfp = suitableFP.Count;
            if (nfp == 0) 
            {
                all = new BonusCandidate[0];
                return;
            }
            EventWrangler ew = game.Ew();
            Rng.Vein vein = Rng.Vein.BONUS;
            double fpSetIn = 5.0;
            double startSim = game.Sim().StartTime();
            double startBonus = startSim - START_BEFORE;
            double duration = game.Sim().EndTime() - startSim;
            double hours = duration / 3600.0;
            int nBonus = KTools.RoundToInt(hours * N_BONUS_PER_HOUR);
            all = new BonusCandidate[nBonus];
            for (int i = 0; i < all.Length; i++)
            {
                IFootpath footpath = suitableFP[Rng.NextInt(vein, nfp)];
                double len = footpath.Length();
                double midSection = len - (2 * fpSetIn);
                double distance = midSection < 0 ? len / 2 : fpSetIn + (Rng.NextDouble(vein) * midSection);
                int reward = Reward(Rng.NextDouble(vein));
                double dawn = startBonus + (duration * Rng.NextDouble(vein));
                double dusk = dawn + (MIN_BONUS_DAY + ((MAX_BONUS_DAY - MIN_BONUS_DAY) * Rng.NextDouble(vein)));
                BonusCandidate bc = new BonusCandidate(footpath, distance, reward, dawn, dusk);
                all[i] = bc;
                if (dawn < startSim - BonusCandidate.SPAWN_TIME) 
                {
                    available.Add(bc);
                    ew.FireDynamicObjectCreated(bc);
                }
                else hidden.Add(bc);
            }
        }
    }
}
